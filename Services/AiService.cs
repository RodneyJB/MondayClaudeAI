using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MondayClaudeAI.Services;

public class AiService
{
    private readonly HttpClient _http;

    public AiService(HttpClient http)
    {
        _http = http;
    }

    public bool IsConfigured()
    {
        var apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        return !string.IsNullOrWhiteSpace(apiKey);
    }

    public async Task<string> RunPrompt(string systemPrompt, string userPrompt, double temperature = 0.2)
    {
        var apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("AI_API_KEY is missing.");
        }

        var baseUrl = Environment.GetEnvironmentVariable("AI_BASE_URL")?.TrimEnd('/') ?? "https://api.openai.com/v1";
        var model = Environment.GetEnvironmentVariable("AI_MODEL") ?? "gpt-4o-mini";

        var payload = new
        {
            model,
            temperature,
            messages = new object[]
            {
                new { role = "system", content = string.IsNullOrWhiteSpace(systemPrompt) ? "You are a reliable automation assistant." : systemPrompt },
                new { role = "user", content = userPrompt ?? string.Empty }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"AI request failed ({(int)response.StatusCode}): {raw}");
        }

        using var doc = JsonDocument.Parse(raw);

        if (doc.RootElement.TryGetProperty("choices", out var choices) &&
            choices.ValueKind == JsonValueKind.Array &&
            choices.GetArrayLength() > 0)
        {
            var first = choices[0];
            if (first.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var content))
            {
                if (content.ValueKind == JsonValueKind.String)
                {
                    return content.GetString() ?? string.Empty;
                }

                // Some providers return content blocks.
                if (content.ValueKind == JsonValueKind.Array)
                {
                    var parts = new List<string>();
                    foreach (var block in content.EnumerateArray())
                    {
                        if (block.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String)
                        {
                            parts.Add(textEl.GetString() ?? string.Empty);
                        }
                    }
                    return string.Join("\n", parts);
                }
            }
        }

        return string.Empty;
    }
}
