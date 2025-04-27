using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SICA.Common.Constants;
using SICA.Tools.BlobStore;
using SICA.Tools.TextExtraction;
using SICA.Tools.VectorStore;

namespace SICA.Api.Features.Files.Upload.Contracts;

public static class UploadFilesRequest
{
    public record Params
    {
        [FromForm]
        public required IFormFileCollection Files { get; init; }

        [FromHeader(Name = "Accept-Language")]
        public string? AcceptLanguage { get; init; }

        internal class Validator : AbstractValidator<Params>
        {
            private const long MaxFileSize = 10 * FileSizes.OneMB;

            public Validator()
            {
                RuleFor(x => x.Files)
                    .NotNull()
                    .Must(x => x.Count > 0)
                    .WithMessage("Files cannot be empty.")
                    .Must(x => x.All(f => f.Length <= MaxFileSize))
                    .WithMessage($"Maximum file size: {MaxFileSize} bytes.")
                    .Must(x => x
                        .Select(f => Path.GetExtension(f.FileName))
                        .All(TextExtractionFileType.All.Contains))
                    .WithMessage($"Supported file extensions: '{string.Join("', '", TextExtractionFileType.All)}'.");
            }
        }
    }

    public record Services
    {
        [FromServices]
        public required IEnumerable<ITextExtractionStrategy> TextExtractionStrategies { get; init; }
        [FromServices]
        public required IVectorStore VectorStore { get; init; }
        [FromServices]
        public required IBlobStore BlobStore { get; init; }
        [FromServices]
        public required TimeProvider TimeProvider { get; init; }
        [FromServices]
        public required IOptions<ApiSettings> ApiSettings { get; init; }
        [FromServices]
        public required IValidator<Params> Validator { get; init; }
        [FromServices]
        public required ILogger<UploadFiles> Logger { get; init; }
        public required CancellationToken CancellationToken { get; init; }
    }
}