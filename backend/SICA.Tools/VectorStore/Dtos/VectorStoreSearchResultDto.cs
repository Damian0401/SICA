namespace SICA.Tools.VectorStore.Dtos;

public record VectorStoreSearchResultDto<T>(IEnumerable<VectorStoreSearchResultDto<T>.Result<T>> Payloads)
{
    public record Result<TPayload>(Guid Id, float Score, TPayload Payload);
};
