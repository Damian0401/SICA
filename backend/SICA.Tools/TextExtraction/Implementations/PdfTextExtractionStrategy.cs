using Microsoft.Extensions.Options;
using SICA.Common.Shared;
using SICA.Tools.TextExtraction.Dtos;
using SkiaSharp;
using TesseractOCR;
using TesseractOCR.Enums;

namespace SICA.Tools.TextExtraction.Implementations;

internal sealed class PdfTextExtractionStrategy : ITextExtractionStrategy
{
    private readonly TextExtractionSettings _options;

    public PdfTextExtractionStrategy(IOptions<TextExtractionSettings> options)
    {
        _options = options.Value;
    }

    public string Types => TextExtractionFileType.Pdf;

    public Task<Result<TextExtractionResponseDto>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default)
    {
        using var engine = new Engine(_options.TesseractDataPath, GetLanguage(language));
        List<string> contents = [];

#pragma warning disable CA1416 // Validate platform compatibility
        var pages = PDFtoImage.Conversion.ToImages(inputStream);
#pragma warning restore CA1416 // Validate platform compatibility
        foreach (var page in pages)
        {
            using var data = page.Encode(SKEncodedImageFormat.Png, 100);
            using var dataStream = data.AsStream();
            using var imageStream = new MemoryStream();
            dataStream.CopyTo(imageStream);
            using var image = TesseractOCR.Pix.Image.LoadFromMemory(imageStream);
            using var content = engine.Process(image);
            contents.Add(content.Text);
        }
        
        var mergedContents = string.Join("\n", contents);
        if (string.IsNullOrWhiteSpace(mergedContents))
        {
            var failureResult = Result<TextExtractionResponseDto>.Failure(
                "Extracted text is empty.");
            return Task.FromResult(failureResult);
        }

        var result = Result<TextExtractionResponseDto>.Success(new TextExtractionResponseDto
        {
            Content = mergedContents,
            ContentType = "application/pdf"
        });
        return Task.FromResult(result);
    }

    private static Language GetLanguage(TextExtractionLanguage language)
    {
        return language switch
        {
            TextExtractionLanguage.English => Language.English,
            TextExtractionLanguage.Polish => Language.Polish,
            _ => throw new ArgumentOutOfRangeException(
                nameof(language),
                language,
                "Unknown text extraction language.")
        };
    }
}