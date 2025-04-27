using SICA.Api.Dtos;
using SICA.Api.Features.Files.Upload.Contracts;
using SICA.Common.Shared;
using SICA.Tools.BlobStore;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Download;

public class DownloadFile
{
    public static Task<IResult> HandleAsync(
        [AsParameters] DownloadFileRequest.Params @params,
        [AsParameters] DownloadFileRequest.Services services)
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
                onSuccess: value => Results.Stream(
                    value.Stream, 
                    contentType: value.ContentType),
                onFailure: (message, exception) =>
                    HandleError(
                        message,
                        exception,
                        services.Logger));
    }

    private async static Task<Result<DownloadFileResponse>> ProcessAsync(
        DownloadFileRequest.Params @params,
        DownloadFileRequest.Services services)
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
            return Result<DownloadFileResponse>.Failure(filePayloadResult);
        }
        return await DownloadFileAsync(
            filePayloadResult.Value,
            services);        
    }

    private static Task<Result<DownloadFileResponse>> DownloadFileAsync(
        FilePayload filePayload,
        DownloadFileRequest.Services services)
    {
        var blobOptions = new IBlobStore.GetOptions
        {
            FileName = filePayload.FileId.ToString(),
            ContainerName = services.ApiSettings.Value.FilesContainerName,
        };
        return services.BlobStore.GetFileAsync(blobOptions, services.CancellationToken)
            .MatchAsync(
                onSuccess: stream => Result<DownloadFileResponse>.Success(
                    new DownloadFileResponse(
                        stream,
                        filePayload.FileName,
                        filePayload.ContentType)),
                onFailure: Result<DownloadFileResponse>.Failure);
    }

    private static IResult HandleError(
        string message,
        Exception exception,
        ILogger<DownloadFile> logger)
    {
        logger.LogError(
            exception,
            "Unable to download file: {Message}",
            message);
        return Results.Problem(
            message, statusCode: exception is null
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest);
    }
}