using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class CopyCommandHandler(string source, string destination, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _source = source;
    private readonly string _destination = destination;

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "copy";
            Logger.LogInformation("Running copy");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { source = _source, destination = _destination }),
                    Encoding.UTF8, 
                    "application/json")
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch(HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            Logger.LogError("Source model not found: {Model}", _source);
            ConsoleHelper.ShowError($"Source model not found: {_source}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run copy");
            ConsoleHelper.ShowError("Failed to run copy");
        }
    }
}