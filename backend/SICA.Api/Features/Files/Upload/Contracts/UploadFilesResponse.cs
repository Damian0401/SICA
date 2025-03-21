namespace SICA.Api.Features.Files.Upload.Contracts;

public record UploadFilesResponse(IEnumerable<UploadFilesResponse.File> Files)
{
    public record File(Guid Id, string FileName);
}