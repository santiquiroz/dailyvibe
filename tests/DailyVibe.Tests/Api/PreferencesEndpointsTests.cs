using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Api;

public sealed class PreferencesEndpointsTests : IClassFixture<DailyVibeApiFactory>
{
    private readonly DailyVibeApiFactory _factory;

    public PreferencesEndpointsTests(DailyVibeApiFactory factory)
    {
        _factory = factory;
        _factory.Llm.Reset();
        _factory.Llm.Setup(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mensaje generado");
    }

    [Fact]
    public async Task Update_returns_401_without_a_token()
    {
        using var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/preferences", new { defaultIntent = "estoico" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_changes_the_intent_used_for_new_messages()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/preferences", new { defaultIntent = "estoico, una frase" });
        (await client.PostAsync("/api/messages/generate", content: null)).EnsureSuccessStatusCode();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _factory.Llm.Verify(l => l.GenerateMessageAsync("estoico, una frase", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_with_a_blank_intent_returns_400_problem_details()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync("/api/preferences", new { defaultIntent = "" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").TryGetProperty("DefaultIntent", out _).Should().BeTrue();
    }
}
