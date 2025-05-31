namespace SICA.Tools.SemanticVectorGenerator.Dtos;

public record SemanticVectorGeneratorResponseDto
{
    public required float[] Vector { get; init; }
    public required string Summary { get; init; }
}