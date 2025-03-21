namespace SICA.Tools.TextExtraction;

public class TextExtractionSettings
{
    public const string SectionName = "TextExtraction";

    public required string TesseractDataPath { get; init; }
}