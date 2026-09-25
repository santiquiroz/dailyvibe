using System.Net;
using System.Text.Json;
using DailyVibe.Application.Exceptions;
using DailyVibe.Infrastructure.LmStudio;
using DailyVibe.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DailyVibe.Tests.LmStudio;

public sealed class LmStudioHttpClientTests
{
    private const string ConfiguredModel = "test/configured-model";

    [Fact]
    public async Task Returns_trimmed_message_content_on_success()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, CompletionWithContent("  Respira hondo.  \n"));

        var message = await CreateClient(handler).GenerateMessageAsync("calma");

        message.Should().Be("Respira hondo.");
    }

    [Fact]
    public async Task Sends_configured_model_settings_system_prompt_and_intent_to_chat_completions()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, CompletionWithContent("ok"));

        await CreateClient(handler).GenerateMessageAsync("motivación para el lunes");

        handler.LastRequestUri!.AbsolutePath.Should().Be("/v1/chat/completions");
        using var body = JsonDocument.Parse(handler.LastRequestBody!);
        body.RootElement.GetProperty("model").GetString().Should().Be(ConfiguredModel);
        body.RootElement.GetProperty("max_tokens").GetInt32().Should().Be(42);
        body.RootElement.GetProperty("temperature").GetDouble().Should().Be(0.3);
        var messages = body.RootElement.GetProperty("messages");
        messages[0].GetProperty("role").GetString().Should().Be("system");
        messages[0].GetProperty("content").GetString().Should().Contain("asistente de bienestar");
        messages[1].GetProperty("role").GetString().Should().Be("user");
        messages[1].GetProperty("content").GetString().Should().Be("motivación para el lunes");
    }

    public static TheoryData<string> ResponsesWithoutContent => new()
    {
        """{"choices":[]}""",
        """{"choices":[{"message":{"content":null}}]}""",
        """{"choices":[{"message":{"content":"   "}}]}""",
        """{"choices":[{"message":null}]}""",
        """{}""",
        "null",
    };

    [Theory]
    [MemberData(nameof(ResponsesWithoutContent))]
    public async Task Throws_LlmUnavailable_when_response_has_no_message_content(string body)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, body);

        var act = () => CreateClient(handler).GenerateMessageAsync("calma");

        await act.Should().ThrowAsync<LlmUnavailableException>();
    }

    [Fact]
    public async Task Throws_LlmUnavailable_when_response_is_not_json()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, "<html>proxy error</html>");

        var act = () => CreateClient(handler).GenerateMessageAsync("calma");

        await act.Should().ThrowAsync<LlmUnavailableException>().WithInnerException(typeof(JsonException));
    }

    [Fact]
    public async Task Throws_LlmUnavailable_naming_the_status_when_server_returns_500()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.InternalServerError, """{"error":"boom"}""");

        var act = () => CreateClient(handler).GenerateMessageAsync("calma");

        (await act.Should().ThrowAsync<LlmUnavailableException>()).WithMessage("*500*");
    }

    [Fact]
    public async Task Throws_LlmUnavailable_when_server_is_unreachable()
    {
        var handler = StubHttpMessageHandler.Throwing(new HttpRequestException("Connection refused"));

        var act = () => CreateClient(handler).GenerateMessageAsync("calma");

        await act.Should().ThrowAsync<LlmUnavailableException>().WithInnerException(typeof(HttpRequestException));
    }

    [Fact]
    public async Task Throws_LlmUnavailable_when_request_times_out()
    {
        var client = CreateClient(StubHttpMessageHandler.NeverResponding(), timeout: TimeSpan.FromMilliseconds(50));

        var act = () => client.GenerateMessageAsync("calma");

        await act.Should().ThrowAsync<LlmUnavailableException>().WithInnerException(typeof(TaskCanceledException));
    }

    [Fact]
    public async Task Propagates_cancellation_requested_by_the_caller()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var act = () => CreateClient(StubHttpMessageHandler.NeverResponding()).GenerateMessageAsync("calma", cancellation.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    private static string CompletionWithContent(string content) =>
        JsonSerializer.Serialize(new { choices = new[] { new { message = new { role = "assistant", content } } } });

    private static LmStudioHttpClient CreateClient(HttpMessageHandler handler, TimeSpan? timeout = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://lmstudio.test"),
            Timeout = timeout ?? TimeSpan.FromSeconds(5),
        };
        var options = Options.Create(new LmStudioOptions { Model = ConfiguredModel, MaxTokens = 42, Temperature = 0.3 });
        return new LmStudioHttpClient(httpClient, options);
    }
}
