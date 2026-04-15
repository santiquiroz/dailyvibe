using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DailyVibe.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DailyVibe.Infrastructure.Services;

public class LmStudioHttpClient(HttpClient httpClient, IConfiguration configuration) : ILmStudioClient
{
    private const string SystemPrompt =
        "Eres un asistente de bienestar. Responde SOLO con el mensaje solicitado, " +
        "sin saludos, sin explicaciones, sin comillas. Sé conciso.";

    public async Task<string> GenerateMessageAsync(string intent, CancellationToken cancellationToken = default)
    {
        var model = configuration["LmStudio:Model"] ?? "google/gemma-4-26b-a4b";

        var payload = new
        {
            model,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = intent }
            },
            temperature = 0.7,
            max_tokens = 150
        };

        var response = await httpClient.PostAsJsonAsync("/v1/chat/completions", payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from LM Studio.");

        return result.Choices[0].Message.Content.Trim();
    }

    // --- response DTOs (internal, no need to expose) ---
    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<Choice> Choices);

    private sealed record Choice(
        [property: JsonPropertyName("message")] ChatMessage Message);

    private sealed record ChatMessage(
        [property: JsonPropertyName("content")] string Content);
}
