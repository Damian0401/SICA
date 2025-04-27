namespace SICA.Tools.TextExtraction.Dtos;

public record TextExtractionResponseDto
{
    public required string Content { get; init; }
    public required string ContentType { get; init; }
}