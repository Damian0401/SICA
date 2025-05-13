using SICA.Api.Dtos;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Api.Features.Files.Search.Contracts;

public record SearchFilesResponse(IEnumerable<SearchFilesResponse.File> Files, uint Limit)
{
    public int Count => Files.Count();
    public record File(Guid Id, string FileName, DateTimeOffset CreatedAt, string ContentLanguage, float Score);

    public static SearchFilesResponse FromDto(VectorStoreSearchResultDto<FilePayload> dto, uint limit)
    {
        return new SearchFilesResponse(
            dto.Payloads.Select(x =>
                new File(x.Id, x.Payload.FileName, x.Payload.CreatedAt, x.Payload.ContentLanguage, x.Score)), 
                limit);
    }
}