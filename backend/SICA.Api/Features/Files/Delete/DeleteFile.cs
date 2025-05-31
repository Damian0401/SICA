using SICA.Api.Dtos;
using SICA.Api.Features.Files.Delete.Contracts;
using SICA.Common.Shared;
using SICA.Tools.BlobStore;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Delete;

public class DeleteFile
{
    public static Task<IResult> HandleAsync(
        [AsParameters] DeleteFileRequest.Params @params,
        [AsParameters] DeleteFileRequest.Services services)
    {
        var result = services.Validator.Validate(@params);
        if (!result.IsValid)
        {
            var problemResult = Results.ValidationProblem(
                result.ToDictionary());
            return Task.FromResult(problemResult);
        }

        return ProcessAsync(@params, services)
            .MatchAsync(
                onSuccess: Results.NoContent,
                onFailure: (message, exception) =>
                    HandleError(
                        message,
                        exception,
                        services.Logger));
    }

    private static async Task<Result> ProcessAsync(
            DeleteFileRequest.Params @params,
            DeleteFileRequest.Services services)
    {
        var options = new IVectorStore.GetByIdOptions
        {
            Id = @params.FileId,
            CollectionName = services.ApiSettings.Value.FilesCollectionName,
        };
        var filePayloadResult = await services.VectorStore.GetByIdAsync<FilePayload>(
            options,
            services.CancellationToken);
        if (filePayloadResult.IsFailure)
        {
            return Result.Failure(filePayloadResult);
        }

        var vectorDeleteOptions = new IVectorStore.DeleteOptions
        {
            CollectionName = services.ApiSettings.Value.FilesCollectionName,
            Ids = [@params.FileId]
        };
        var vectorDeleteResult = await services.VectorStore.DeleteByIdsAsync(
            vectorDeleteOptions,
            services.CancellationToken);
        if (vectorDeleteResult.IsFailure)
        {
            return Result.Failure(vectorDeleteResult);
        }

        var blobDeleteOptions = new IBlobStore.DeleteOptions
        {
            ContainerName = services.ApiSettings.Value.FilesContainerName,
            FileName = filePayloadResult.Value.FileId.ToString(),
        };
        var blobDeleteResult = await services.BlobStore.DeleteFileAsync(
            blobDeleteOptions,
            services.CancellationToken);
        if (blobDeleteResult.IsFailure)
        {
            return Result.Failure(blobDeleteResult);
        }

        return Result.Success();
    }

    private static IResult HandleError(
        string message,
        Exception? exception,
        ILogger<DeleteFile> logger)
    {
        logger.LogError(
            exception,
            "Unable to delete file: {Message}.",
            message);
        return Results.Problem(
            message, statusCode: exception is null
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest);
    }
}