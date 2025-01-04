using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Humanizer;

using Microsoft.Extensions.Logging;
using RemoteLlama.Helpers;

namespace RemoteLlama.CommandHandlers;

internal class ShowCommandHandler(string model, ShowCommandHandler.FieldToShow fieldToShow, ILogger logger, IConsoleHelper consoleHelper) : BaseCommandHandler(logger, consoleHelper)
{
    internal enum FieldToShow
    {
        All,
        License,
        Modelfile,
        Parameters,
        System,
        Template
    }

    private readonly string _model = model;
    private readonly FieldToShow _fieldToShow = fieldToShow;

    protected override async Task ExecuteImplAsync()
    {
        try
        {
            var url = ConfigManager.Url + "show";
            Logger.LogInformation("Running show");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { model = _model, verbose = true }),
                    Encoding.UTF8, 
                    "application/json")
            };

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var showResponse = await JsonSerializer.DeserializeAsync<ModelInformation>(await response.Content.ReadAsStreamAsync()).ConfigureAwait(false);

            if (showResponse == null)
            {
                Logger.LogError("Failed to deserialize response");
                ConsoleHelper.ShowError("Failed to deserialize response");
                return;
            }

            Logger.LogInformation(showResponse.ToOutputString(_fieldToShow));
        }
        catch(HttpRequestException ex) when (ex.Message.Contains("404"))
        {
            Logger.LogError("Model not found: {Model}", _model);
            ConsoleHelper.ShowError($"Model not found: {_model}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to run show");
            ConsoleHelper.ShowError("Failed to run show");
        }
    }
}

internal class ModelInformation
{
    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("details")]
    public ModelDetails? Details { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    [JsonPropertyName("model_info")]
    public JsonElement? ModelInfo { get; set; }

    [JsonPropertyName("modelfile")]
    public string? ModelFile { get; set; }

    [JsonPropertyName("parameters")]
    public string? Parameters { get; set; }

    [JsonPropertyName("template")]
    public string? Template { get; set; }

    public string ToOutputString(ShowCommandHandler.FieldToShow fieldToShow)
    {
        var sb = new StringBuilder();

        switch (fieldToShow)
        {
            case ShowCommandHandler.FieldToShow.All:
                WriteAll(sb);
                break;
            case ShowCommandHandler.FieldToShow.License:
                if (License != null)
                {
                    sb.AppendLine(License);
                    sb.AppendLine();
                }
                break;
            case ShowCommandHandler.FieldToShow.Modelfile:
                if (ModelFile != null)
                {
                    sb.AppendLine(ModelFile);
                    sb.AppendLine();
                }
                break;
            case ShowCommandHandler.FieldToShow.Parameters:
                if (Parameters != null)
                {
                    sb.AppendLine(Parameters);
                    sb.AppendLine();
                }
                break;
            case ShowCommandHandler.FieldToShow.System:
                if (System != null)
                {
                    sb.AppendLine(System);
                    sb.AppendLine();
                }
                break;
            case ShowCommandHandler.FieldToShow.Template:
                if (Template != null)
                {
                    sb.AppendLine(Template);
                    sb.AppendLine();
                }
                break;
        }

        return sb.ToString();
    }

    private void WriteAll(StringBuilder sb)
    {
        if (Details != null && ModelInfo != null)
        {
            var architecture = ModelInfo?.GetProperty("general.architecture").GetString();

            sb.AppendLine("Model:");
            sb.AppendLine($"\tarchitecture\t{architecture}");
            sb.AppendLine($"\tparameters\t{ModelInfo?.GetProperty("general.parameter_count").GetDouble().ToMetric(MetricNumeralFormats.WithSpace | MetricNumeralFormats.UseShortScaleWord, decimals: 1)}");
            sb.AppendLine($"\tcontext length\t{ModelInfo?.GetProperty($"{architecture}.context_length").GetInt64()}");
            sb.AppendLine($"\tcontext length\t{ModelInfo?.GetProperty($"{architecture}.embedding_length").GetInt64()}");
            sb.AppendLine($"\tquantization\t{Details.QuantizationLevel}");
            sb.AppendLine();
        }

        if (System != null)
        {
            sb.AppendLine("System:");
            sb.AppendLine($"\t{System}");
            sb.AppendLine();
        }

        if (License != null)
        {
            sb.AppendLine("License:");

            // split the license into lines, remove empty lines and trim whitespace, adding the first two lines only
            foreach (var line in License.Split('\n').Select(l => l.Trim()).Where(l => !string.IsNullOrWhiteSpace(l)).Take(2))
            {
                sb.AppendLine($"\t{line}");
            }

            sb.AppendLine();
        }
    }
}