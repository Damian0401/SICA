namespace SICA.Tools.BlobStore;

public class BlobStoreSettings
{
    public const string SectionName = "BlobStore";

    public required string ConnectionString { get; init; }
}