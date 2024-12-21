using Microsoft.Extensions.Logging;
using Moq;
using RemoteLlama.CommandHandlers;
using Xunit;

namespace RemoteLlama.Tests.CommandHandlers
{
    public class SetUrlCommandHandlerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public SetUrlCommandHandlerTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Theory]
        [InlineData("https://test.com/api/", "https://test.com/api/")]
        [InlineData("https://test.com/api", "https://test.com/api/")]  // Should add trailing slash
        [InlineData("https://test.com", "https://test.com/api/")]  // Should add trailing slash
        [InlineData("https://test.com/", "https://test.com/api/")]  // Should add trailing slash
        [InlineData("http://localhost:11434", "http://localhost:11434/api/")]  // Local development URL
        public async Task ExecuteAsync_ShouldSetUrlInConfigManager(string inputUrl, string expectedUrl)
        {
            // Arrange
            var handler = new SetUrlCommandHandler(inputUrl, _loggerMock.Object);

            // Act
            await handler.ExecuteAsync();

            // Assert
            Assert.Equal(expectedUrl, ConfigManager.Url);
        }
    }
} 