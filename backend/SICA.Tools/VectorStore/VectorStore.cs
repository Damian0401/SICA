using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SICA.Common.Shared;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Tools.VectorStore;

internal class VectorStore : IVectorStore
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

    public async Task<Result<Guid>> SaveAsync<T>(
        string key,
        T payload,
        IVectorStore.VectorStoreSaveOptions options,
        CancellationToken cancellationToken = default)
    {
        try
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
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to initialize vector store: {e.Message}.");
            return Result.Failure<Guid>("Failed to initialize vector store.");
        }

        var vector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(
            key,
            cancellationToken: cancellationToken);

        var pointId = Guid.CreateVersion7();
        var point = new PointStruct
        {
            Id = pointId,
            Vectors = vector.ToArray(),
            Payload =
            {
                ["data"] = JsonSerializer.Serialize(payload)
            }
        };
        await _qdrantClient.UpsertAsync(
            options.CollectionName,
            [point],
            cancellationToken: cancellationToken);

        return Result.Success(pointId);
    }

    public async Task<Result> DeleteByIdsAsync(
        IEnumerable<Guid> ids,
        IVectorStore.VectorStoreDeleteOptions options,
        CancellationToken cancellationToken = default)
    {
        foreach (var id in ids)
        {
            await _qdrantClient.DeleteAsync(
                options.CollectionName,
                id,
                cancellationToken: cancellationToken);
        }
        return Result.Success();
    }

    public async Task<Result<VectorStoreSearchResultDto<T>>> SearchAsync<T>(
        string key,
        IVectorStore.VectorStoreSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        var collectionExists = await _qdrantClient.CollectionExistsAsync(
            options.CollectionName,
            cancellationToken);
        if (!collectionExists)
        {
            _logger.LogError("Collection {CollectionName} does not exist.", options.CollectionName);
            return Result.Failure<VectorStoreSearchResultDto<T>>(
                $"Collection {options.CollectionName} does not exist.");
        }

        var vector = await _embeddingGenerator.GenerateEmbeddingVectorAsync(
            key,
            cancellationToken: cancellationToken);

        var points = await _qdrantClient.SearchAsync(
            options.CollectionName,
            vector,
            limit: options.MatchesCount,
            cancellationToken: cancellationToken);

        var dtos = points.Select(GetVectorData<T>);

        return dtos.MatchAll(
            onSuccess: values => Result.Success(
                new VectorStoreSearchResultDto<T>(values)),
            onFailure: (IEnumerable<string> errors) => Result.Failure<VectorStoreSearchResultDto<T>>(
                $"[{string.Join(", ", errors)}]"));
    }

    private Result<VectorStoreSearchResultDto<T>.Result<T>> GetVectorData<T>(ScoredPoint p)
    {
        var id = Guid.Parse(p.Id.Uuid);
        var data = p.Payload["data"].StringValue;

        try
        {
            var payload = JsonSerializer.Deserialize<T>(data)!;
            var result = new VectorStoreSearchResultDto<T>.Result<T>(id, p.Score, payload);
            return Result.Success(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to deserialize vector {Data}: {Message}.", data, e.Message);
            return Result.Failure<VectorStoreSearchResultDto<T>.Result<T>>(
                $"Failed to deserialize vector {data}.");
        }
    }
}