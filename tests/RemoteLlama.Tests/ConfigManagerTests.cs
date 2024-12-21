using Xunit;

namespace RemoteLlama.Tests;

public class ConfigManagerTests
{
    [Fact]
    public void Url_WhenSet_PersistsValue()
    {
        // Arrange
        const string expectedUrl = "https://test.com";

        // Act
        ConfigManager.Url = expectedUrl;
        var actualUrl = ConfigManager.Url;

        // Assert
        Assert.Equal(expectedUrl, actualUrl);
    }
} 