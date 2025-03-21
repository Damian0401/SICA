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
        group.MapGet("/", SearchFiles.HandleAsync);
        return group;
    }
}