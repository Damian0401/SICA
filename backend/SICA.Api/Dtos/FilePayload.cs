namespace SICA.Api.Dtos;

public record FilePayload
{
    public required string FileName { get; set; }
    public required string ContentLanguage { get; set; }
    public required string ContentType { get; set; }
    public required string Summary { get; set; }
    public required Guid FileId { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}