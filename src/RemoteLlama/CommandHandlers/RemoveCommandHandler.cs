namespace RemoteLlama.CommandHandlers;

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

internal class RemoveCommandHandler(string modelId, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private readonly string _modelId = modelId;

    protected override async Task ExecuteImplAsync()
    {
         var url = ConfigManager.Url + "delete";
        Logger.LogInformation("Removing model: {ModelId} from {Url}", _modelId, url); 
        
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(20);
        
        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { model = _modelId }), 
                Encoding.UTF8, 
                "application/json")
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        // Check for a 404 - this is an acceptable error code
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Logger.LogError("Error: model '{ModelId}' not found", _modelId);
            ConsoleHelper.ShowError($"Model '{_modelId}' not found.");
            return;
        }

        response.EnsureSuccessStatusCode();
    }
} 