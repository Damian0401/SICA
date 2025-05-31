using SICA.Common.Shared;
using SICA.Tools.SemanticVectorGenerator.Dtos;

namespace SICA.Tools.SemanticVectorGenerator;

public interface ISemanticVectorGenerator
{
    Task<Result<SemanticVectorGeneratorResponseDto>> GenerateVectorAsync(
        string text,
        string prompt,
        CancellationToken cancellationToken = default);
}