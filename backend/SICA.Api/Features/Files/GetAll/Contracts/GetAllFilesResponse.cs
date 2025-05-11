using SICA.Api.Dtos;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Api.Features.Files.GetAll.Contracts;

public record GetAllFilesResponse(IEnumerable<GetAllFilesResponse.File> Files, uint Limit, Guid? Offset)
{
    public int Count => Files.Count();
    public record File(Guid Id, string FileName, string? ContentLanguage, DateTimeOffset CreatedAt);
    public static GetAllFilesResponse FromDto(
        VectorStoreGetAllResultDto<FilePayload> dto, 
        uint limit, 
        Guid? offset)
    {
        return new GetAllFilesResponse(
            dto.Payloads.Select(x =>
                new File(x.Id, x.Payload.FileName, x.Payload.ContentLanguage, x.Payload.CreatedAt)), 
                limit, 
                offset);
    }
}
