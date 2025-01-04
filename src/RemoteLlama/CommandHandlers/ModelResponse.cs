using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteLlama.CommandHandlers;

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

    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; set; }

    [JsonPropertyName("size_vram")]
    public long SizeVram { get; set; }

    public override string ToString()
    {
        return $"Name: {Name}, ModelName: {ModelName}, Size: {Size}, Digest: {Digest}, Details: {Details}, ExpiresAt: {ExpiresAt}, ModifiedAt: {ModifiedAt}, SizeVram: {SizeVram}";
    }
}
