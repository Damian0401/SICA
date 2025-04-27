using SICA.Common.Shared;
using SICA.Tools.TextExtraction.Dtos;

namespace SICA.Tools.TextExtraction;

public interface ITextExtractionStrategy
{
    string Types { get; }
    Task<Result<TextExtractionResponseDto>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default);
}