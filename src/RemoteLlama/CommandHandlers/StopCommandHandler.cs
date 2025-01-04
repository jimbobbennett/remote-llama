using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class StopCommandHandler(string model, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _model = model;

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "generate";
            Logger.LogInformation("Running stop");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { model = _model, keep_alive = 0 }),
                    Encoding.UTF8, 
                    "application/json")
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch(HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            Logger.LogError("Model not found: {Model}", _model);
            ConsoleHelper.ShowError($"Model not found: {_model}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run stop");
            ConsoleHelper.ShowError("Failed to run stop");
        }
    }
}