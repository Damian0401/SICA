namespace SICA.Tools.TextExtraction;

public static class TextExtractionFileType
{
    public const string Pdf = ".pdf";
    public const string Docx = ".docx";
    public const string Txt = ".txt";

    public static IReadOnlyCollection<string> All =>
    [
        Pdf, Docx, Txt
    ];
}