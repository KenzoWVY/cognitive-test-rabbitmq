using System.Text.Json.Serialization;

namespace Worker.Models;

public class OllamaRequest
{
    [JsonPropertyName("model")]

    public required string Model { get; set; }

    [JsonPropertyName("prompt")]
    public required string Prompt { get; set; }

    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }

    [JsonPropertyName("topic")]
    public required string Topic { get; set; }

    [JsonPropertyName("stream")] public bool Stream { get; set; } = false;

    [JsonPropertyName("format")]
    public string Format { get; set; } = "json";
}