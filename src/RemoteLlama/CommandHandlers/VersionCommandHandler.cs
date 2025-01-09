using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class VersionCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    internal class VersionInformation
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
    
    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "version";
            Logger.LogInformation("Running version");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var versionResponse = await JsonSerializer.DeserializeAsync<VersionInformation>(await response.Content.ReadAsStreamAsync()).ConfigureAwait(false);

            if (versionResponse == null)
            {
                Logger.LogError("Failed to deserialize version response");
                ConsoleHelper.ShowError("Failed to deserialize version response");
                return;
            }

            ConsoleHelper.WriteLine($"ollama version is {versionResponse.Version}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to get Ollama version");
            ConsoleHelper.ShowError("Failed get Ollama version");
        }
    }
}