using SICA.Api.Dtos;
using SICA.Api.Features.Files.Upload.Contracts;
using SICA.Api.Helpers;
using SICA.Common.Shared;
using SICA.Tools.BlobStore;
using SICA.Tools.TextExtraction;
using SICA.Tools.TextExtraction.Dtos;
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
            await using var fileStream = formFile.OpenReadStream();
            // Need to buffer the file to avoid closing the stream, cased by Spire.Doc library ignoring the leaveOpen flag.
            var buffer = await GetBytesAsync(formFile, services.CancellationToken);
            await using var extractStream = new MemoryStream(buffer);
            var extractResult = await ExtractTextAsync(
                formFile.FileName,
                extractStream,
                SupportedLanguages[language],
                strategies,
                services);
            if (extractResult.IsFailure)
            {
                await DeleteFilesAsync(savedFiles, services);
                services.Logger.LogError(
                    extractResult.Exception,
                    "Unable to extract text from '{FileName}': {Message}",
                    formFile.FileName,
                    extractResult.ErrorMessage);
                return Result<UploadFilesResponse>.Failure(
                    $"Unable to extract text from '{formFile.FileName}'.");
            }

            await using var saveStream = new MemoryStream(buffer);
            var saveResult = await SaveResultAsync(
                extractResult.Value!, 
                language, 
                formFile.FileName,
                saveStream,
                services);
            if (saveResult.IsFailure)
            {
                await DeleteFilesAsync(savedFiles, services);
                services.Logger.LogError(
                    saveResult.Exception,
                    "Unable to save file '{FileName}': {Message}",
                    formFile.FileName,
                    saveResult.ErrorMessage);
                return Result<UploadFilesResponse>.Failure(
                    $"Unable to save file '{formFile.FileName}'.");
            }

            var savedFile = new UploadFilesResponse.File(saveResult.Value, formFile.FileName);
            savedFiles.Add(savedFile);
        }

        var response = new UploadFilesResponse(savedFiles);
        return Result<UploadFilesResponse>.Success(response);
    }

    private static async Task DeleteFilesAsync(
        List<UploadFilesResponse.File> savedFiles,
        UploadFilesRequest.Services services)
    {
        var vectorDeleteOptions = new IVectorStore.DeleteOptions
        {
            Ids = savedFiles.Select(s => s.Id).ToList(),
            CollectionName = services.ApiSettings.Value.FilesCollectionName
        };
        var vectorDeleteResult = await services.VectorStore.DeleteByIdsAsync(
            vectorDeleteOptions,
            services.CancellationToken);
        if (vectorDeleteResult.IsFailure)
        {
            services.Logger.LogError(
                vectorDeleteResult.Exception,
                "Unable to delete files: {Message}.",
                vectorDeleteResult.ErrorMessage);
        }
        
        foreach (var file in savedFiles)
        {
            var blobDeleteOptions = new IBlobStore.DeleteOptions
            {
                ContainerName = services.ApiSettings.Value.FilesContainerName,
                FileName = file.Id.ToString()
            };
            var deleteResult = await services.BlobStore.DeleteFileAsync(
                blobDeleteOptions,  
                services.CancellationToken);
            if (deleteResult.IsFailure)
            {
                services.Logger.LogError(
                    deleteResult.Exception,
                    "Unable to delete file '{FileName}': {Message}",
                    file.FileName,
                    deleteResult.ErrorMessage);
            }
        }
    }

    private static async Task<Result<Guid>> SaveResultAsync(
        TextExtractionResponseDto responseDto,
        string contentLanguage,
        string fileName,
        Stream fileStream,
        UploadFilesRequest.Services services)
    {
        var payload = new FilePayload
        {
            FileName = fileName,
            CreatedAt = services.TimeProvider.GetUtcNow(),
            ContentLanguage = contentLanguage,
            ContentType = responseDto.ContentType,
            FileId = Guid.NewGuid(),
        };
        var saveOptions = new IVectorStore.SaveOptions<FilePayload>
        {
            Key = responseDto.Content,
            Payload = payload,
            CollectionName = services.ApiSettings.Value.FilesCollectionName
        };
        var vectorSaveResult = await services.VectorStore.SaveAsync(
            saveOptions,
            services.CancellationToken);
        if (vectorSaveResult.IsFailure)
        {
            return Result<Guid>.Failure(
                vectorSaveResult.ErrorMessage,
                vectorSaveResult.Exception);
        }
        var blobSaveOptions = new IBlobStore.SaveOptions
        {
            FileName = payload.FileId.ToString(),
            ContainerName = services.ApiSettings.Value.FilesContainerName
        };
        var blobSaveResult = await services.BlobStore.SaveFileAsync(
            fileStream,
            blobSaveOptions,
            services.CancellationToken);
        if (blobSaveResult.IsFailure)
        {
            var deleteOptions = new IVectorStore.DeleteOptions
            {
                Ids = [payload.FileId],
                CollectionName = services.ApiSettings.Value.FilesCollectionName
            };
            await services.VectorStore.DeleteByIdsAsync(
                deleteOptions,
                services.CancellationToken);
            return Result<Guid>.Failure(
                blobSaveResult.ErrorMessage,
                blobSaveResult.Exception);
        }
        return Result<Guid>.Success(vectorSaveResult.Value);
    }

    private static async Task<Result<TextExtractionResponseDto>> ExtractTextAsync(
        string fileName,
        Stream fileStream,
        TextExtractionLanguage language,
        Dictionary<string, ITextExtractionStrategy> strategies,
        UploadFilesRequest.Services services)
    {
        var type = Path.GetExtension(fileName);
        var strategy = strategies[type];
        var extractedTextResponseDto = await strategy.ExtractTextAsync(
            fileStream,
            language,
            services.CancellationToken);
        return extractedTextResponseDto;
    }

    private static readonly Dictionary<string, TextExtractionLanguage> SupportedLanguages = new()
    {
        {"en-US", TextExtractionLanguage.English},
        {"pl-PL", TextExtractionLanguage.Polish}
    };
    private static string GetRequestLanguage(
        string? acceptLanguage,
        ApiSettings apiSettings)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
        {
            return apiSettings.DefaultAcceptLanguage;
        }

        var acceptLanguageResult = HeaderHelper.ParseAcceptLanguage(acceptLanguage);
        if (acceptLanguageResult.IsFailure)
        {
            return apiSettings.DefaultAcceptLanguage;
        }

        var selectedAcceptLanguage = acceptLanguageResult.Value!
            .OrderByDescending(a => a.Value)
            .Select(a => a.Language)
            .FirstOrDefault(a => SupportedLanguages.ContainsKey(a), apiSettings.DefaultAcceptLanguage);
        return selectedAcceptLanguage;
    }

    private static async Task<byte[]> GetBytesAsync(
        IFormFile formFile, 
        CancellationToken cancellationToken)
    {
        await using var ms = new MemoryStream();
        await formFile.CopyToAsync(ms, cancellationToken);
        return ms.ToArray();
    }
}