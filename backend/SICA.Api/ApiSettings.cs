namespace SICA.Api;

public class ApiSettings
{
    public const string SectionName = "Api";
    public required string FilesCollectionName { get; init; }
    public required string DefaultAcceptLanguage { get; init; }
}