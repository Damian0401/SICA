using SICA.Api.Features.Files.Dtos;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Api.Features.Files.Search.Contracts;

public record SearchFilesResponse(IEnumerable<SearchFilesResponse.File> Files)
{
    public int Count => Files.Count();
    public record File(Guid Id, string FileName, float Score, string Content);

    public static SearchFilesResponse FromDto(VectorStoreSearchResultDto<FilePayload> dto)
    {
        return new SearchFilesResponse(
            dto.Payloads.Select(x =>
                new File(x.Id, x.Payload.FileName, x.Score, x.Payload.Content)));
    }
}