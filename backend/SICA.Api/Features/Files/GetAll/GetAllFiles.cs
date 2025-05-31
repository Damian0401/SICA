using SICA.Api.Dtos;
using SICA.Api.Features.Files.GetAll.Contracts;
using SICA.Common.Shared;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.GetAll;

public class GetAllFiles
{
    public static Task<IResult> HandleAsync(
        [AsParameters] GetAllFilesRequest.Params @params,
        [AsParameters] GetAllFilesRequest.Services services)
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
                onSuccess: Results.Ok,
                onFailure: (message, exception) =>
                    HandleError(
                        message,
                        exception,
                        services.Logger));
    }

    private static Task<Result<GetAllFilesResponse>> ProcessAsync(
        GetAllFilesRequest.Params @params,
        GetAllFilesRequest.Services services)
    {
        var limit = @params.Limit ?? GetAllFilesConstants.DefaultLimit;
        var offset = @params.Offset;
        var options = new IVectorStore.GetAllOptions
        {
            CollectionName = services.ApiSettings.Value.FilesCollectionName,
            Limit = limit,
            Offset = offset,
        };
        return services.VectorStore.GetAllAsync<FilePayload>(options, services.CancellationToken)
            .MatchAsync(
                onSuccess: result =>
                    Result<GetAllFilesResponse>.Success(GetAllFilesResponse.FromDto(result, limit, offset)),
                onFailure: message =>
                    Result<GetAllFilesResponse>.Failure(message));
    }

    private static IResult HandleError(
        string message,
        Exception? exception,
        ILogger<GetAllFiles> logger)
    {
        logger.LogError(
            exception,
            "Unable to get all files: {Message}.",
            message);
        return Results.Problem(
            message, statusCode: StatusCodes.Status400BadRequest);
    }
}