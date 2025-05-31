using SICA.Api.Dtos;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Api.Features.Files.Search.Contracts;

public record SearchFilesResponse(IEnumerable<SearchFilesResponse.File> Files, uint Limit)
{
    public int Count => Files.Count();
    public record File
    {
        public required Guid Id { get; init; }
        public required string FileName { get; init; }
        public required string Summary { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required string ContentLanguage { get; init; }
        public required float Score { get; init; }
    }

    public static SearchFilesResponse FromDto(VectorStoreSearchResultDto<FilePayload> dto, uint limit)
    {
        return new SearchFilesResponse(
            dto.Payloads.Select(x =>
                new File
                {
                    Id = x.Id,
                    FileName = x.Payload.FileName,
                    Summary = x.Payload.Summary,
                    CreatedAt = x.Payload.CreatedAt,
                    ContentLanguage = x.Payload.ContentLanguage,
                    Score = x.Score
                }),
                limit);
    }
}