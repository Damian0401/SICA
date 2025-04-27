using SICA.Common.Shared;
using SICA.Tools.TextExtraction.Dtos;
using Spire.Doc;

namespace SICA.Tools.TextExtraction.Implementations;

internal sealed class DocxTextExtractionStrategy : ITextExtractionStrategy
{
    public string Types => TextExtractionFileType.Docx;
    public Task<Result<TextExtractionResponseDto>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default)
    {
        using var document = new Document(inputStream);

        var content = document.GetText();
        if (string.IsNullOrWhiteSpace(content))
        {
            var failureResult = Result<TextExtractionResponseDto>.Failure(
                "Extracted text is empty.");
            return Task.FromResult(failureResult);
        }

        var result = Result<TextExtractionResponseDto>.Success(new TextExtractionResponseDto
        {
            Content = content,
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        });
        return Task.FromResult(result);
    }
}