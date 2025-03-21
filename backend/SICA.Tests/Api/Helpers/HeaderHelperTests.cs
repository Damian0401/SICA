using SICA.Api.Helpers;

namespace SICA.Tests.Api.Helpers;

public class HeaderHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void HeadersHelper_ParseAcceptLanguage_ReturnFailureForEmptyHeader(string? acceptLanguage)
    {
        // Act
        var result = HeaderHelper.ParseAcceptLanguage(acceptLanguage);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void HeadersHelper_ParseAcceptLanguage_ReturnCorrectValues()
    {
        // Arrange
        var acceptLanguage = "da, en-GB;q=0.8, en;q=0.7";

        List<HeaderHelper.AcceptLanguagePart> expectedValues =
        [
            new("da"),
            new("en-GB", 0.8f),
            new("en", 0.7f)
        ];

        // Act
        var result = HeaderHelper.ParseAcceptLanguage(acceptLanguage);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValues, result.Value);
    }
}