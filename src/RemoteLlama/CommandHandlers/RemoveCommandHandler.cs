namespace RemoteLlama.CommandHandlers;

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

public class RemoveCommandHandler : BaseCommandHandler
{
    private readonly string _modelId;
    private readonly ConsoleHelper _consoleHelper;

    public RemoveCommandHandler(string modelId, ILogger logger, ConsoleHelper consoleHelper) 
        : base(logger)
    {
        _modelId = modelId;
        _consoleHelper = consoleHelper;
    }

    protected override async Task ExecuteImplAsync()
    {
         var url = ConfigManager.Url + "delete";
        _logger.LogInformation("Removing model: {ModelId} from {Url}", _modelId, url); 
        
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
            _logger.LogInformation("Error: model '{ModelId}' not found", _modelId);
            return;
        }

        response.EnsureSuccessStatusCode();
    }
} 