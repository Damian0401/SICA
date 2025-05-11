using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.GetAll.Contracts;

public class GetAllFilesRequest
{
    public record Params
    {
        [FromQuery]
        public uint? Limit { get; init; }

        [FromQuery]
        public Guid? Offset { get; init; }

        internal class Validator : AbstractValidator<Params>
        {
            public Validator()
            {
                RuleFor(x => x.Limit)
                    .LessThanOrEqualTo(100u)
                    .GreaterThan(0u);
            }
        }
    }

    public record Services
    {
        [FromServices]
        public required IVectorStore VectorStore { get; init; }
        [FromServices]
        public required IValidator<Params> Validator { get; init; }
        [FromServices]
        public required IOptions<ApiSettings> ApiSettings { get; init; }
        [FromServices]
        public required ILogger<GetAllFiles> Logger { get; init; }
        public required CancellationToken CancellationToken { get; init; }
    }
}
