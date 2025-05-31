using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SICA.Tools.BlobStore;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Delete.Contracts;

public class DeleteFileRequest
{
    public record Params
    {
        [FromRoute]
        public required Guid FileId { get; init; }

        internal class Validator : AbstractValidator<Params>
        {
            public Validator()
            {
                RuleFor(x => x.FileId)
                    .NotEmpty();
            }
        }
    }

    public record Services
    {
        [FromServices]
        public required IVectorStore VectorStore { get; init; }
        [FromServices]
        public required IBlobStore BlobStore { get; init; }   
        [FromServices]
        public required IOptions<ApiSettings> ApiSettings { get; init; }
        [FromServices]
        public required IValidator<Params> Validator { get; init; }
        [FromServices]
        public required ILogger<DeleteFile> Logger { get; init; }
        public required CancellationToken CancellationToken { get; init; }
    }
}