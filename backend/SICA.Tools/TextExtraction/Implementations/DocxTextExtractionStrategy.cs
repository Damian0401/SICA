using SICA.Common.Shared;
using Spire.Doc;

namespace SICA.Tools.TextExtraction.Implementations;

internal class DocxTextExtractionStrategy : ITextExtractionStrategy
{
    public string Types => TextExtractionFileType.Docx;
    public Task<Result<string>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default)
    {
        using var document = new Document(inputStream);

        var content = document.GetText();
        if (string.IsNullOrWhiteSpace(content))
        {
            var failureResult = Result.Failure<string>(
                "Extracted text is empty.");
            return Task.FromResult(failureResult);
        }

        var result = Result.Success(content);
        return Task.FromResult(result);
    }
}