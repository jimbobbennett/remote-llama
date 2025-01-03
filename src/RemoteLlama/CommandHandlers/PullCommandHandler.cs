namespace RemoteLlama.CommandHandlers;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RemoteLlama.Helpers;

public class PullCommandHandler(string modelId, bool insecure, ILogger logger, ConsoleHelper consoleHelper) : BaseCommandHandler(logger)
{
    private readonly string _modelId = modelId;
    private readonly bool _insecure = insecure;
    private readonly ConsoleHelper _consoleHelper = consoleHelper;

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "pull";
            _logger.LogInformation("Pulling model: {ModelId} from {Url}", _modelId, url);
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { model = _modelId, insecure = _insecure }), 
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pull model: {ModelId}", _modelId);
            ConsoleHelper.ShowError($"Failed to pull model {_modelId}.");
        }
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