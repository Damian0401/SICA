using SICA.Common.Shared;
using SICA.Tools.VectorStore.Dtos;

namespace SICA.Tools.VectorStore;

public interface IVectorStore
{
    Task<Result<VectorStoreGetAllResultDto<T>>> GetAllAsync<T>(GetAllOptions options, CancellationToken cancellationToken = default);
    Task<Result<Guid>> SaveAsync<T>(SaveOptions<T> options, CancellationToken cancellationToken = default);
    Task<Result> DeleteByIdsAsync(DeleteOptions options, CancellationToken cancellationToken = default);    
    Task<Result<VectorStoreSearchResultDto<T>>> SearchAsync<T>(SearchOptions options, CancellationToken cancellationToken = default);
    Task<Result<T>> GetByIdAsync<T>(GetByIdOptions options, CancellationToken cancellationToken = default);

    public record SaveOptions<T>
    {
        public required string CollectionName { get; init; }
        public required T Payload { get; init; }
        public required float[] Vector { get; init; }
    }
    public record GetAllOptions
    {
        public required string CollectionName { get; init; }
        public uint Limit { get; set; } = 100;
        public Guid? Offset { get; set; }
    };
    public record GetByIdOptions
    {
        public required Guid Id { get; init; }
        public required string CollectionName { get; init; }
    }
    public record DeleteOptions
    {
        public required IEnumerable<Guid> Ids { get; init; }
        public required string CollectionName { get; init; }
    }
    public record SearchOptions
    {
        public required string CollectionName { get; init; }
        public required float[] Vector { get; init; }
        public required uint Limit { get; init; }
    }
}

