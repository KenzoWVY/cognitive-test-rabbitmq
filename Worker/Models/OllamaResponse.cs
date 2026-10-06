using System.Text.Json.Serialization;

namespace Worker.Models;

public class OllamaResponse
{
    [JsonPropertyName("question")]
    public required string Question { get; set; }

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = [];

    [JsonPropertyName("correct_index")]
    public int CorrectIndex { get; set; }
}