using SICA.Api.Features.Files.Dtos;
using SICA.Api.Features.Files.Search.Contracts;
using SICA.Common.Shared;
using SICA.Tools.VectorStore;
using SICA.Tools.VectorStore.Dtos;

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

    private static Task<Result<SearchFilesResponse>> ProcessAsync(
        SearchFilesRequest.Params @params,
        SearchFilesRequest.Services services)
    {
        var options = new IVectorStore.VectorStoreSearchOptions
        {
            CollectionName = services.ApiSettings.Value.FilesCollectionName
        };
        if (@params.Count.HasValue)
        {
            options.MatchesCount = @params.Count.Value;
        }
        return services.VectorStore.SearchAsync<FilePayload>(@params.Query, options)
            .MatchAsync(
                onSuccess: result =>
                    Result.Success(SearchFilesResponse.FromDto(result)),
                onFailure: message =>
                    Result.Failure<SearchFilesResponse>(message));
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
        return Results.BadRequest($"Failed to search for query '{query}'");
    }
}