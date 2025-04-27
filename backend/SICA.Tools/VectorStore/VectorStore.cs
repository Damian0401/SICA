using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SICA.Common.Shared;
using SICA.Tools.Abstraction;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Tools.VectorStore;

internal sealed class VectorStore : BaseSafeTool<VectorStore>, IVectorStore
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly QdrantClient _qdrantClient;
    private readonly ILogger<VectorStore> _logger;
    private readonly VectorStoreSettings _settings;

    public VectorStore(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        QdrantClient qdrantClient,
        IOptions<VectorStoreSettings> settings,
        ILogger<VectorStore> logger)
    {
        _embeddingGenerator = embeddingGenerator;
        _qdrantClient = qdrantClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<Result<Guid>> SaveAsync<T>(
        IVectorStore.SaveOptions<T> options,
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var alreadyExists = await _qdrantClient.CollectionExistsAsync(
                options.CollectionName,
                cancellationToken);
            if (!alreadyExists)
            {
                await _qdrantClient.RecreateCollectionAsync(options.CollectionName,
                    new VectorParams
                    {
                        Size = _settings.EmbeddingModelVectorSize,
                        Distance = Distance.Cosine
                    }, cancellationToken: cancellationToken);
            }

            var vector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(
                options.Key,
                cancellationToken: cancellationToken);

            var pointId = Guid.CreateVersion7();
            var point = new PointStruct
            {
                Id = pointId,
                Vectors = vector.ToArray(),
                Payload =
                {
                    ["data"] = JsonSerializer.Serialize(options.Payload)
                }
            };
            await _qdrantClient.UpsertAsync(
                options.CollectionName,
                [point],
                cancellationToken: cancellationToken);

            return Result<Guid>.Success(pointId);
        }, _logger);
    }

    public async Task<Result> DeleteByIdsAsync(
        IVectorStore.DeleteOptions options,
        CancellationToken cancellationToken = default)
    {
        foreach (var id in options.Ids)
        {
            await _qdrantClient.DeleteAsync(
                options.CollectionName,
                id,
                cancellationToken: cancellationToken);
        }
        return Result.Success();
    }

    public Task<Result<VectorStoreSearchResultDto<T>>> SearchAsync<T>(
        IVectorStore.SearchOptions options,
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var collectionExistsResult = await CheckCollectionExistsAsync(
                options.CollectionName,
                cancellationToken);
            if (collectionExistsResult.IsFailure)
            {
                return Result<VectorStoreSearchResultDto<T>>.Failure(
                    collectionExistsResult);
            }

            var vector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(
                options.Key,
                cancellationToken: cancellationToken);

            var points = await _qdrantClient.SearchAsync(
                options.CollectionName,
                vector,
                limit: options.Limit,
                cancellationToken: cancellationToken);

            var dtos = points.Select(GetVectorDataWithScore<T>);

            return dtos.MatchAll(
                onSuccess: values => Result<VectorStoreSearchResultDto<T>>.Success(
                    new VectorStoreSearchResultDto<T>(values)),
                onFailure: (IEnumerable<string> errors) => Result<VectorStoreSearchResultDto<T>>.Failure(
                    $"[{string.Join(", ", errors)}]"));
        }, _logger);
    }

    private Result<VectorStoreSearchResultDto<T>.Result<T>> GetVectorDataWithScore<T>(ScoredPoint p)
    {
        var id = Guid.Parse(p.Id.Uuid);
        var data = p.Payload["data"].StringValue;

        try
        {
            var payload = JsonSerializer.Deserialize<T>(data)!;
            var result = new VectorStoreSearchResultDto<T>.Result<T>(id, p.Score, payload);
            return Result<VectorStoreSearchResultDto<T>.Result<T>>.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to deserialize vector {Data}: {Message}.", data, e.Message);
            return Result<VectorStoreSearchResultDto<T>.Result<T>>.Failure(
                $"Failed to deserialize vector {data}.");
        }
    }

    public Task<Result<VectorStoreGetAllResultDto<T>>> GetAllAsync<T>(
        IVectorStore.GetAllOptions options, 
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var collectionExistsResult = await CheckCollectionExistsAsync(
                options.CollectionName,
                cancellationToken);
            if (collectionExistsResult.IsFailure)
            {
                return Result<VectorStoreGetAllResultDto<T>>.Failure(
                    collectionExistsResult);
            }
            
            var points = await _qdrantClient.ScrollAsync(
                options.CollectionName,
                limit: options.Limit,
                offset: options.Offset,
                cancellationToken: cancellationToken);

            var dtos = points.Result.Select(GetVectorData<T>);

            return dtos.MatchAll(
                onSuccess: values => Result<VectorStoreGetAllResultDto<T>>.Success(
                    new VectorStoreGetAllResultDto<T>(values)),
                onFailure: (IEnumerable<string> errors) => Result<VectorStoreGetAllResultDto<T>>.Failure(
                    $"[{string.Join(", ", errors)}]"));
        }, _logger);
    }

    private Result<VectorStoreGetAllResultDto<T>.Result<T>> GetVectorData<T>(RetrievedPoint p)
    {
        var id = Guid.Parse(p.Id.Uuid);
        var data = p.Payload["data"].StringValue;

        try
        {
            var payload = JsonSerializer.Deserialize<T>(data)!;
            var result = new VectorStoreGetAllResultDto<T>.Result<T>(id, payload);
            return Result<VectorStoreGetAllResultDto<T>.Result<T>>.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to deserialize vector {Data}: {Message}.", data, e.Message);
            return Result<VectorStoreGetAllResultDto<T>.Result<T>>.Failure(
                $"Failed to deserialize vector {data}.");
        }
    }

    public Task<Result<T>> GetByIdAsync<T>(
        IVectorStore.GetByIdOptions options, 
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var collectionExistsResult = await CheckCollectionExistsAsync(
                options.CollectionName,
                cancellationToken);
            if (collectionExistsResult.IsFailure)
            {
                return Result<T>.Failure(
                    collectionExistsResult);
            }

            var points = await _qdrantClient.RetrieveAsync(
                options.CollectionName,
                options.Id,
                cancellationToken: cancellationToken);
            if (points is null || points.Count == 0)
            {
                _logger.LogError("Point {Id} not found in collection {CollectionName}.", options.Id, options.CollectionName);
                return Result<T>.Failure($"Point {options.Id} not found in collection {options.CollectionName}.");
            }

            var data = GetVectorData<T>(points[0]);
            return data.Match(
                onSuccess: result => Result<T>.Success(result.Payload),
                onFailure: message => Result<T>.Failure(message));
        }, _logger);
    }

    private async Task<Result> CheckCollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var collectionExists = await _qdrantClient.CollectionExistsAsync(
            collectionName,
            cancellationToken);
        if (!collectionExists)
        {
            _logger.LogError("Collection '{CollectionName}' does not exist.", collectionName);
            return Result.Failure(
                $"Collection '{collectionName}' does not exist.");
        }
        return Result.Success();
    } 
}