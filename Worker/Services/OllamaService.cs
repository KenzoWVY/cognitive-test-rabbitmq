using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts;
using Worker.Models;

namespace Worker.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;

    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:11434");
        _httpClient.Timeout = TimeSpan.FromMinutes(5);

        _modelName = configuration["OLLAMA_MODEL"] ?? "phi4-mini:latest";
    }

    public async Task<QuestionResponseMessage> GenerateQuestionAsync(QuestionRequestMessage request)
    {
        var prompt = $@"
            You are a cognitive test generator. Create a {request.Difficulty} difficulty multiple-choice question about {request.Topic}.
            You must output ONLY valid JSON using this exact schema:
            {{
              ""Question"": ""The question text"",
              ""Options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
              ""CorrectIndex"": 0
            }}
            Ensure CorrectIndex is a number between 0 and 3.";

        var apiRequest = new
        {
            model = _modelName,
            prompt = prompt,
            stream = false,
            format = "json"
        };

        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("/api/generate", apiRequest);
            httpResponse.EnsureSuccessStatusCode();

            var fullJson = await httpResponse.Content.ReadFromJsonAsync<JsonDocument>() ??
                           throw new JsonException("Failed to read JSON from API response");

            var responseString = fullJson.RootElement.GetProperty("response").GetString() ??
                                 throw new JsonException("Failed to extract response from JSON");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsedResponse = JsonSerializer.Deserialize<OllamaResponse>(responseString, options) ??
                                 throw new JsonException("Failed to deserialize OllamaResponse");

            return new QuestionResponseMessage
            {
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                QuestionText = parsedResponse.Question,
                Options = parsedResponse.Options,
                CorrectIndex = parsedResponse.CorrectIndex,
                IsError = false
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ollama generation failed: {ex.Message}");
            return new QuestionResponseMessage
            {
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                QuestionText = $"Error: Failed to parse AI response. ({ex.Message})",
                Options = ["Continue"],
                CorrectIndex = 0,
                IsError = true
            };
        }
    }
}