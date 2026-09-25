using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Infrastructure.LmStudio;
using Microsoft.Extensions.Options;

namespace DailyVibe.Infrastructure.Services;

public class LmStudioHttpClient(HttpClient httpClient, IOptions<LmStudioOptions> options) : ILmStudioClient
{
    private const string CompletionsPath = "/v1/chat/completions";

    private const string SystemPrompt =
        "Eres un asistente de bienestar. Responde SOLO con el mensaje solicitado, " +
        "sin saludos, sin explicaciones, sin comillas. Sé conciso.";

    public async Task<string> GenerateMessageAsync(string intent, CancellationToken cancellationToken = default)
    {
        var completion = await RequestCompletionAsync(BuildRequest(intent), cancellationToken);
        return ExtractMessage(completion);
    }

    private ChatCompletionRequest BuildRequest(string intent)
    {
        var settings = options.Value;
        return new ChatCompletionRequest(
            settings.Model,
            [new ChatMessage("system", SystemPrompt), new ChatMessage("user", intent)],
            settings.Temperature,
            settings.MaxTokens);
    }

    private async Task<ChatCompletionResponse?> RequestCompletionAsync(
        ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(CompletionsPath, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new LlmUnavailableException($"LM Studio responded with HTTP {(int)response.StatusCode}.");
            return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new LlmUnavailableException("LM Studio could not be reached.", ex);
        }
        catch (JsonException ex)
        {
            throw new LlmUnavailableException("LM Studio returned a response that is not valid JSON.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new LlmUnavailableException(
                $"LM Studio did not respond within {httpClient.Timeout.TotalSeconds} seconds.", ex);
        }
    }

    private static string ExtractMessage(ChatCompletionResponse? completion)
    {
        var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
            throw new LlmUnavailableException("LM Studio returned no message content.");
        return content.Trim();
    }

    private sealed record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("max_tokens")] int MaxTokens);

    private sealed record ChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<Choice>? Choices);

    private sealed record Choice(
        [property: JsonPropertyName("message")] ChatMessage? Message);

    private sealed record ChatMessage(
        [property: JsonPropertyName("role")] string? Role,
        [property: JsonPropertyName("content")] string? Content);
}
