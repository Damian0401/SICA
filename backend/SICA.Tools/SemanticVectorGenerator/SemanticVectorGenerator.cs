using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using SICA.Common.Shared;
using SICA.Tools.Abstraction;
using SICA.Tools.SemanticVectorGenerator.Dtos;

namespace SICA.Tools.SemanticVectorGenerator;

internal class SemanticVectorGenerator : BaseSafeTool<SemanticVectorGenerator>, ISemanticVectorGenerator
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IChatClient _chatClient;

    public SemanticVectorGenerator(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IChatClient chatClient,
        ILogger<SemanticVectorGenerator> logger)
        : base(logger)
    {
        _embeddingGenerator = embeddingGenerator;
        _chatClient = chatClient;
    }


    public Task<Result<SemanticVectorGeneratorResponseDto>> GenerateVectorAsync(
        string text,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        return SafeExecuteAsync(async () =>
        {
            var summarizationResponse = await _chatClient.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, prompt),
                new ChatMessage(ChatRole.User, text)
            ],
            new ChatOptions
            {
                Temperature = 0,
            },
            cancellationToken: cancellationToken);


            var embeddingResponse = await _embeddingGenerator.GenerateEmbeddingAsync(
                summarizationResponse.Text,
                cancellationToken: cancellationToken);

            var responseDto = new SemanticVectorGeneratorResponseDto
            {
                Summary = summarizationResponse.Text,
                Vector = embeddingResponse.Vector.ToArray()
            };

            return Result<SemanticVectorGeneratorResponseDto>.Success(responseDto); 
        });
    }
}