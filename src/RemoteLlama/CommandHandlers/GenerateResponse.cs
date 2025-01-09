using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteLlama.CommandHandlers;

public class GenerateChatResponseBase
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

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
        return $"Model: {Model}, CreatedAt: {CreatedAt}, Done: {Done}, Context: {string.Join(", ", Context ?? [])}, TotalDuration: {TotalDuration}, LoadDuration: {LoadDuration}, PromptEvalCount: {PromptEvalCount}, PromptEvalDuration: {PromptEvalDuration}, EvalCount: {EvalCount}, EvalDuration: {EvalDuration}";
    }
}

public class GenerateResponse : GenerateChatResponseBase
{

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    public override string ToString()
    {
        return $"Response: {Response}, {base.ToString()}";
    }

    public static GenerateResponse? DeserializeJson(string json) => JsonSerializer.Deserialize<GenerateResponse>(json);
}

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }


    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }

    public ChatMessage()
    {
    }

    public override string ToString()
    {
        return $"Role: {Role}, Content: {Content}";
    }

    public static ChatMessage? DeserializeJson(string json) => JsonSerializer.Deserialize<ChatMessage>(json);
}

public class ChatResponse : GenerateChatResponseBase
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }

    public override string ToString()
    {
        return $"Message: {Message}, {base.ToString()}";
    }

    public static ChatResponse? DeserializeJson(string json) => JsonSerializer.Deserialize<ChatResponse>(json);
}