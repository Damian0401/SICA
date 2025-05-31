namespace SICA.Tools.SemanticVectorGenerator;

public class SemanticVectorGeneratorSettings
{
    public const string SectionName = "SemanticVectorGenerator";

    public required string OllamaUrl { get; init; }
    public required string EmbeddingModelId { get; init; }
    public required string ChatModelId { get; init; }
}