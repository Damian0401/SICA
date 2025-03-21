using SICA.Common.Shared;

namespace SICA.Tools.TextExtraction.Implementations;

internal sealed class TxtTextExtractionStrategy : ITextExtractionStrategy
{
    public string Types => TextExtractionFileType.Txt;
    public async Task<Result<string>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default)
    {
        using var streamReader = new StreamReader(inputStream);

        var content = await streamReader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<string>("Extracted text is empty.");
        }

        return Result.Success(content);
    }
}