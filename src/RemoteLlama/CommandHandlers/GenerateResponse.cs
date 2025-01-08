using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteLlama.CommandHandlers;

public class GenerateResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("context")]
    public List<int>? Context { get; set; }

    [JsonPropertyName("total_duration")]
    public long TotalDuration { get; set; }

    [JsonPropertyName("load_duration")]
    public long LoadDuration { get; set; }

    [JsonPropertyName("prompt_eval_count")]
    public long PromptEvalCount { get; set; }

    [JsonPropertyName("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }

    [JsonPropertyName("eval_count")]
    public long EvalCount { get; set; }

    [JsonPropertyName("eval_duration")]
    public long EvalDuration { get; set; }

    public override string ToString()
    {
        return $"Model: {Model}, CreatedAt: {CreatedAt}, Response: {Response}, Done: {Done}, Context: {string.Join(", ", Context ?? [])}, TotalDuration: {TotalDuration}, LoadDuration: {LoadDuration}, PromptEvalCount: {PromptEvalCount}, PromptEvalDuration: {PromptEvalDuration}, EvalCount: {EvalCount}, EvalDuration: {EvalDuration}";
    }

    public static GenerateResponse? DeserializeJson(string json)
    {
        return JsonSerializer.Deserialize<GenerateResponse>(json);
    }
}