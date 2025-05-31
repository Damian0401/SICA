namespace SICA.Tools.VectorStore;

public class VectorStoreSettings
{
    public const string SectionName = "VectorStore";

    public required string QdrantUrl { get; init; }
    public required string QdrantApiKey { get; init; }
    public required ulong VectorSize { get; init; }
}