namespace SICA.Api.Features.Files.Search;

internal static class SearchFilesConstants
{
    internal const int DefaultLimit = 3;
    internal const string Prompt = """
    Extract a concise list of key skills required for the following job description.
    Only return the skill names as a comma-separated list.
    Do not include any explanations, job titles, or descriptions.
    """;
}