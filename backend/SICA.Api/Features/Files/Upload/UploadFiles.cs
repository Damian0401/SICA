using SICA.Api.Features.Files.Dtos;
using SICA.Api.Features.Files.Upload.Contracts;
using SICA.Api.Helpers;
using SICA.Common.Shared;
using SICA.Tools.TextExtraction;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Upload;

public class UploadFiles
{
    public static Task<IResult> HandleAsync(
        [AsParameters] UploadFilesRequest.Params @params,
        [AsParameters] UploadFilesRequest.Services services)
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
                onFailure: error =>
                    Results.Problem(
                        error, statusCode: StatusCodes.Status400BadRequest));
    }

    private static async Task<Result<UploadFilesResponse>> ProcessAsync(
        UploadFilesRequest.Params @params,
        UploadFilesRequest.Services services)
    {
        var strategies = services.TextExtractionStrategies
            .ToDictionary(s => s.Types);
        var language = GetRequestLanguage(@params.AcceptLanguage, services.ApiSettings.Value);

        List<UploadFilesResponse.File> savedFiles = [];
        foreach (var formFile in @params.Files)
        {
            var extractResult = await ExtractTextAsync(
                formFile,
                language,
                strategies,
                services);
            if (extractResult.IsFailure)
            {
                await DeleteFilesAsync(savedFiles, services);
                services.Logger.LogError(
                    extractResult.Exception,
                    "Unable to extract text from '{FileName}': {Message}.",
                    formFile.FileName,
                    extractResult.ErrorMessage);
                return Result.Failure<UploadFilesResponse>(
                    $"Unable to extract text from '{formFile.FileName}'.");
            }

            var saveResult = await SaveResultAsync(extractResult.Value!, formFile, services);
            if (saveResult.IsFailure)
            {
                await DeleteFilesAsync(savedFiles, services);
                services.Logger.LogError(
                    saveResult.Exception,
                    "Unable to save file '{FileName}': {Message}.",
                    formFile.FileName,
                    saveResult.ErrorMessage);
                return Result.Failure<UploadFilesResponse>(
                    $"Unable to save file '{formFile.FileName}'.");
            }

            var savedFile = new UploadFilesResponse.File(saveResult.Value, formFile.FileName);
            savedFiles.Add(savedFile);
        }

        var response = new UploadFilesResponse(savedFiles);
        return Result.Success(response);
    }

    private static Task DeleteFilesAsync(
        List<UploadFilesResponse.File> savedFiles,
        UploadFilesRequest.Services services)
    {
        var deleteOptions = new IVectorStore.VectorStoreDeleteOptions(
            services.ApiSettings.Value.FilesCollectionName);
        return services.VectorStore.DeleteByIdsAsync(
            savedFiles.Select(s => s.Id).ToList(),
            deleteOptions,
            services.CancellationToken);
    }

    private static Task<Result<Guid>> SaveResultAsync(
        string extractText,
        IFormFile formFile,
        UploadFilesRequest.Services services)
    {
        var payload = new FilePayload(formFile.FileName, extractText);
        var saveOptions = new IVectorStore.VectorStoreSaveOptions(
            services.ApiSettings.Value.FilesCollectionName);
        return services.VectorStore.SaveAsync(
            extractText,
            payload,
            saveOptions,
            services.CancellationToken);
    }

    private static async Task<Result<string>> ExtractTextAsync(
        IFormFile formFile,
        TextExtractionLanguage language,
        Dictionary<string, ITextExtractionStrategy> strategies,
        UploadFilesRequest.Services services)
    {
        var type = Path.GetExtension(formFile.FileName);
        var strategy = strategies[type];
        await using var stream = formFile.OpenReadStream();
        var extractTextAsync = await strategy.ExtractTextAsync(
            stream,
            language,
            services.CancellationToken);
        return extractTextAsync;
    }

    private static readonly Dictionary<string, TextExtractionLanguage> SupportedLanguages = new()
    {
        {"en-US", TextExtractionLanguage.English},
        {"pl-PL", TextExtractionLanguage.Polish}
    };
    private static TextExtractionLanguage GetRequestLanguage(
        string? acceptLanguage,
        ApiSettings apiSettings)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return SupportedLanguages[apiSettings.DefaultAcceptLanguage];
        }

        var acceptLanguageResult = HeaderHelper.ParseAcceptLanguage(acceptLanguage);
        if (acceptLanguageResult.IsFailure)
        {
            return SupportedLanguages[apiSettings.DefaultAcceptLanguage];
        }

        var selectedAcceptLanguage = acceptLanguageResult.Value!
            .OrderByDescending(a => a.Value)
            .Select(a => a.Language)
            .FirstOrDefault(a => SupportedLanguages.ContainsKey(a), apiSettings.DefaultAcceptLanguage);
        return SupportedLanguages[selectedAcceptLanguage];
    }
}