namespace SICA.Tools.VectorStore.Dtos;

public record VectorStoreGetAllResultDto<T>(IEnumerable<VectorStoreGetAllResultDto<T>.Result<T>> Payloads)
{
    public record Result<TPayload>(Guid Id, TPayload Payload);
};
