using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Search.Contracts;

public class SearchFilesRequest
{
    public record Params
    {
        [FromQuery]
        public required string Query { get; init; }

        [FromQuery]
        public ulong? Count { get; init; } = 3;

        internal class Validator : AbstractValidator<Params>
        {
            public Validator()
            {
                RuleFor(x => x.Query)
                    .NotEmpty()
                    .Length(3, 100);
                RuleFor(x => x.Count)
                    .LessThanOrEqualTo(10u);
            }
        }
    }

    public record Services
    {
        [FromServices]
        public required IVectorStore VectorStore { get; init; }
        [FromServices]
        public required IOptions<ApiSettings> ApiSettings { get; init; }
        [FromServices]
        public required IValidator<Params> Validator { get; init; }
        [FromServices]
        public required ILogger<SearchFiles> Logger { get; init; }
        public required CancellationToken CancellationToken { get; init; }
    }
}