using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SICA.Tools.SemanticVectorGenerator;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Search.Contracts;

public class SearchFilesRequest
{
    public record Params
    {
        [FromQuery]
        public required string Query { get; init; }

        [FromQuery]
        public uint? Limit { get; init; }

        internal class Validator : AbstractValidator<Params>
        {
            public Validator()
            {
                RuleFor(x => x.Query)
                    .NotEmpty()
                    .Length(3, 5000);
                RuleFor(x => x.Limit)
                    .GreaterThan(0u)
                    .LessThanOrEqualTo(10u);
            }
        }
    }

    public record Services
    {
        [FromServices]
        public required IVectorStore VectorStore { get; init; }
        [FromServices]
        public required ISemanticVectorGenerator SemanticVectorGenerator { get; init; }
        [FromServices]
        public required IOptions<ApiSettings> ApiSettings { get; init; }
        [FromServices]
        public required IValidator<Params> Validator { get; init; }
        [FromServices]
        public required ILogger<SearchFiles> Logger { get; init; }
        public required CancellationToken CancellationToken { get; init; }
    }
}