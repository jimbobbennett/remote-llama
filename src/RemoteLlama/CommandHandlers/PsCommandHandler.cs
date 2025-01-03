using System.Text.Json;
using System.Text.Json.Serialization;

using Humanizer;

using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class PsCommandHandler(ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    private static readonly string[] Headers = ["NAME", "ID", "SIZE", "UNTIL"];

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "ps";
            Logger.LogInformation("Running PS");
            
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // Get the entire response body
            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(responseStream);
            var responseString = await reader.ReadToEndAsync().ConfigureAwait(false);

            var modelResponse = ModelResponse.DeserializeJson(responseString);

            if (modelResponse == null)
            {
                Logger.LogError("Failed to deserialize response");
                ConsoleHelper.ShowError("Failed to deserialize response");
                return;
            }

            ConsoleHelper.WriteTable(Headers, modelResponse.Models?.Select(m => new List<string>
            {
                m.Name ?? "",
                m.Digest?[..12] ?? "",
                m.Size.Bytes().Humanize("#.##"),
                m.ExpiresAt.Humanize()
            }) ?? []);

            // NAME             ID              SIZE      PROCESSOR    UNTIL
            // llama3:latest    365c0bd3c000    6.2 GB    100% CPU     4 minutes from now
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run ps");
            ConsoleHelper.ShowError("Failed to run ps");
        }
    }
}

internal class ModelDetails
{
    [JsonPropertyName("parent_model")]
    public string? ParentModel { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("families")]
    public List<string>? Families { get; set; }

    [JsonPropertyName("parameter_size")]
    public string? ParameterSize { get; set; }

    [JsonPropertyName("quantization_level")]
    public string? QuantizationLevel { get; set; }

    public override string ToString()
    {
        return $"ParentModel: {ParentModel}, Format: {Format}, Family: {Family}, Families: {string.Join(", ", Families ?? [])}, ParameterSize: {ParameterSize}, QuantizationLevel: {QuantizationLevel}";
    }
}

internal class Model
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("model")]
    public string? ModelName { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("digest")]
    public string? Digest { get; set; }

    [JsonPropertyName("details")]
    public ModelDetails? Details { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [JsonPropertyName("size_vram")]
    public long SizeVram { get; set; }

    public override string ToString()
    {
        return $"Name: {Name}, ModelName: {ModelName}, Size: {Size}, Digest: {Digest}, Details: {Details}, ExpiresAt: {ExpiresAt}, SizeVram: {SizeVram}";
    }
}

internal class ModelResponse
{
    [JsonPropertyName("models")]
    public List<Model>? Models { get; set; }

    public override string ToString()
    {
        return $"Models: {string.Join(", ", Models ?? [])}";
    }

    public static ModelResponse? DeserializeJson(string json)
    {
        return JsonSerializer.Deserialize<ModelResponse>(json);
    }
}