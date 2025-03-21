using SICA.Common.Shared;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Tools.VectorStore;

public interface IVectorStore
{
    Task<Result<Guid>> SaveAsync<T>(string key, T payload, VectorStoreSaveOptions options, CancellationToken cancellationToken = default);
    Task<Result> DeleteByIdsAsync(IEnumerable<Guid> ids, VectorStoreDeleteOptions options, CancellationToken cancellationToken = default);    
    Task<Result<VectorStoreSearchResultDto<T>>> SearchAsync<T>(string key, VectorStoreSearchOptions options, CancellationToken cancellationToken = default);

    public record VectorStoreSaveOptions(string CollectionName);
    public record VectorStoreDeleteOptions(string CollectionName);
    public record VectorStoreSearchOptions
    {
        public required string CollectionName { get; init; }
        public ulong MatchesCount { get; set; } = 3;
    }
}

