using SICA.Api.Features.Files.Delete;
using SICA.Api.Features.Files.Download;
using SICA.Api.Features.Files.GetAll;
using SICA.Api.Features.Files.Search;
using SICA.Api.Features.Files.Upload;

namespace SICA.Api.Features.Files;

public static class FilesModule
{
    private const string Name = "Files";

    public static RouteGroupBuilder MapFilesModule(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/files").WithTags(Name);
        group.MapPost("/", UploadFiles.HandleAsync).DisableAntiforgery();
        group.MapGet("/", GetAllFiles.HandleAsync);
        group.MapGet("/search", SearchFiles.HandleAsync);
        group.MapDelete("/{fileId:guid}", DeleteFile.HandleAsync);
        group.MapGet("/{fileId:guid}/download", DownloadFile.HandleAsync);
        return group;
    }
}