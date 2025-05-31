using SICA.Api.Dtos;
using SICA.Api.Features.Files.Search.Contracts;
using SICA.Common.Shared;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Search;

public class SearchFiles
{
    public static Task<IResult> HandleAsync(
        [AsParameters] SearchFilesRequest.Params @params,
        [AsParameters] SearchFilesRequest.Services services)
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
                        @params.Query,
                        services.Logger));
    }

    private async static Task<Result<SearchFilesResponse>> ProcessAsync(
        SearchFilesRequest.Params @params,
        SearchFilesRequest.Services services)
    {
        var vectorResult = await services.SemanticVectorGenerator
            .GenerateVectorAsync(
                @params.Query,
                SearchFilesConstants.Prompt,
                services.CancellationToken);
        if (vectorResult.IsFailure)
        {
            return Result<SearchFilesResponse>.Failure(vectorResult);
        }

        var limit = @params.Limit ?? SearchFilesConstants.DefaultLimit;
        var options = new IVectorStore.SearchOptions
        {
            Vector = vectorResult.Value.Vector,
            CollectionName = services.ApiSettings.Value.FilesCollectionName,
            Limit = limit,
        };
        return await services.VectorStore.SearchAsync<FilePayload>(options)
            .MatchAsync(
                onSuccess: result =>
                    Result<SearchFilesResponse>.Success(SearchFilesResponse.FromDto(result, limit)),
                onFailure: message =>
                    Result<SearchFilesResponse>.Failure(message));
    }

    private static IResult HandleError(
        string message,
        Exception exception,
        string query,
        ILogger<SearchFiles> logger)
    {
        logger.LogError(
            exception,
            "Failed to search for query '{Query}': {Message}.",
            query,
            message);
        return Results.Problem($"Failed to search for query '{query}'", statusCode: StatusCodes.Status400BadRequest);
    }
}