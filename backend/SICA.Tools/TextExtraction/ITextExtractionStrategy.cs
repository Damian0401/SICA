using SICA.Common.Shared;

namespace SICA.Tools.TextExtraction;

public interface ITextExtractionStrategy
{
    string Types { get; }
    Task<Result<string>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default);
}