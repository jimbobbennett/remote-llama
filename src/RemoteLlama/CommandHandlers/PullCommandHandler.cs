namespace RemoteLlama.CommandHandlers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RemoteLlama.Helpers;

public class PullCommandHandler : BaseCommandHandler
{
    private readonly string _modelId;
    private readonly ConsoleHelper _consoleHelper;

    public PullCommandHandler(string modelId, ILogger logger, ConsoleHelper consoleHelper) 
        : base(logger)
    {
        _modelId = modelId;
        _consoleHelper = consoleHelper;
    }

    protected override async Task ExecuteImplAsync()
    {
        var url = ConfigManager.Url + "pull";
        _logger.LogInformation("Pulling model: {ModelId} from {Url}", _modelId, url);
        
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(20);
        
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { model = _modelId }), 
                Encoding.UTF8, 
                "application/json")
        };

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        await _consoleHelper.RunWithProgressAsync($"Downloading {_modelId}", async updateAction =>
        {
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(line)) continue;

                var progressData = JsonSerializer.Deserialize<ProgressData>(line) 
                    ?? throw new JsonException("Failed to deserialize progress data");
                
                if (progressData.Total > 0)
                {
                    updateAction(progressData.Completed, progressData.Total);
                }
            }
        }).ConfigureAwait(false);
    }

    private class ProgressData
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }

        [JsonPropertyName("total")] 
        public long Total { get; set; }
        
        [JsonPropertyName("completed")]
        public long Completed { get; set; }
    }
} 