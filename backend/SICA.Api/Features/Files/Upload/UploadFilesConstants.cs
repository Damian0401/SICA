namespace SICA.Api.Features.Files.Upload;

internal static class UploadFilesConstants
{
    internal const string Prompt = """
    Extract a concise list of key skills from the following CV. 
    Only return the skill names as a comma-separated list. 
    Do not include any explanations, job titles, or descriptions.
    """;
}