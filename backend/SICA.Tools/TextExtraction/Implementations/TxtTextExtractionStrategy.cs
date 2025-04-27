using SICA.Common.Shared;
using SICA.Tools.TextExtraction.Dtos;

namespace SICA.Tools.TextExtraction.Implementations;

internal sealed class TxtTextExtractionStrategy : ITextExtractionStrategy
{
    public string Types => TextExtractionFileType.Txt;
    public async Task<Result<TextExtractionResponseDto>> ExtractTextAsync(
        Stream inputStream,
        TextExtractionLanguage language = TextExtractionLanguage.English,
        CancellationToken cancellationToken = default)
    {
        using var streamReader = new StreamReader(inputStream);

        var content = await streamReader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<TextExtractionResponseDto>.Failure("Extracted text is empty.");
        }

        return Result<TextExtractionResponseDto>.Success(new TextExtractionResponseDto
        {
            Content = content,
            ContentType = "text/plain"
        });
    }
}