
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SICA.Common.Shared;
using SICA.Tools.Abstraction;

namespace SICA.Tools.BlobStore;

internal sealed class BlobStore : BaseSafeTool<BlobStore>, IBlobStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStore> _logger;

    public BlobStore(
        BlobServiceClient blobServiceClient,
        ILogger<BlobStore> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public Task<Result> DeleteFileAsync(
        IBlobStore.DeleteOptions options, 
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(options.ContainerName);
            var blobClient = containerClient.GetBlobClient(options.FileName);
            var result = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            if (!result)
            {
                _logger.LogError("File {File} not found.", options.FileName);
                return Result.Failure($"File {options.FileName} not found.");
            }
            return Result.Success();
        }, _logger);
    }

    public Task<Result<Stream>> GetFileAsync(
        IBlobStore.GetOptions options, 
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(options.ContainerName);
            var containerExists = await containerClient.ExistsAsync(cancellationToken: cancellationToken);
            if (!containerExists.Value)
            {
                _logger.LogError("Container '{Container}' not found.", options.ContainerName);
                return Result<Stream>.Failure($"Container '{options.ContainerName}' not found.");
            }
            var blobClient = containerClient.GetBlobClient(options.FileName);
            var blobExists = await blobClient.ExistsAsync(cancellationToken: cancellationToken);
            if (!blobExists.Value)
            {
                _logger.LogError("File '{File}' not found.", options.FileName);
                return Result<Stream>.Failure($"File '{options.FileName}' not found.");
            }
            var blobDownloadInfo = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            return Result<Stream>.Success(blobDownloadInfo.Value.Content);
        }, _logger);
    }

    public Task<Result> SaveFileAsync(
        Stream fileStream, 
        IBlobStore.SaveOptions options, 
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(options.ContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(options.FileName);
            await blobClient.UploadAsync(fileStream, cancellationToken: cancellationToken);
            return Result.Success();
        }, _logger);
    }

    
}