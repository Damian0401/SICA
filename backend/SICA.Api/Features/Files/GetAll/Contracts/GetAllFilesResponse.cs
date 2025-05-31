using SICA.Api.Dtos;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Api.Features.Files.GetAll.Contracts;

public record GetAllFilesResponse(IEnumerable<GetAllFilesResponse.File> Files, uint Limit, Guid? Offset)
{
    public int Count => Files.Count();
        public record File
    {
        public required Guid Id { get; init; }
        public required string FileName { get; init; }
        public required string Summary { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required string ContentLanguage { get; init; }
    }
    public static GetAllFilesResponse FromDto(
        VectorStoreGetAllResultDto<FilePayload> dto, 
        uint limit, 
        Guid? offset)
    {
        return new GetAllFilesResponse(
            dto.Payloads.Select(x =>
                new File
                {
                    Id = x.Id,
                    FileName = x.Payload.FileName,
                    Summary = x.Payload.Summary,
                    CreatedAt = x.Payload.CreatedAt,
                    ContentLanguage = x.Payload.ContentLanguage,
                }), 
                limit, 
                offset);
    }
}
