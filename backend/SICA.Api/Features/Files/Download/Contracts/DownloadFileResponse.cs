namespace SICA.Api.Features.Files.Upload.Contracts;

public record DownloadFileResponse(Stream Stream, string FileName, string ContentType);