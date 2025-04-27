using SICA.Common.Shared;

namespace SICA.Tools.BlobStore
{
    public interface IBlobStore
    {
        Task<Result> SaveFileAsync(
            Stream fileStream, 
            SaveOptions options, 
            CancellationToken cancellationToken = default);
        Task<Result<Stream>> GetFileAsync(
            GetOptions options,
            CancellationToken cancellationToken = default);
        Task<Result> DeleteFileAsync(
            DeleteOptions options,
            CancellationToken cancellationToken = default);

        public record SaveOptions
        {
            public required string FileName { get; init; }
            public required string ContainerName { get; init; }
        };
        public record GetOptions
        {
            public required string FileName { get; init; }
            public required string ContainerName { get; init; }
        };
        public record DeleteOptions
        {
            public required string FileName { get; init; }
            public required string ContainerName { get; init; }
        };
    }
}