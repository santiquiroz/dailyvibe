using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DailyVibe.Application.Common;
using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Application.Messages;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DailyVibe.Tests.Api;

public sealed class MessagesEndpointsTests : IClassFixture<DailyVibeApiFactory>
{
    private readonly DailyVibeApiFactory _factory;

    public MessagesEndpointsTests(DailyVibeApiFactory factory)
    {
        _factory = factory;
        _factory.Llm.Reset();
        _factory.Llm.Setup(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Mensaje generado");
    }

    [Theory]
    [InlineData("GET", "/api/messages/today")]
    [InlineData("POST", "/api/messages/generate")]
    [InlineData("GET", "/api/messages/history")]
    public async Task Message_endpoints_return_401_without_a_token(string method, string path)
    {
        using var client = _factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Generate_uses_the_requested_intent_and_returns_the_llm_content()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/messages/generate", new { intent = "  humor seco  " });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = (await response.Content.ReadFromJsonAsync<DailyMessageDto>())!;
        message.Content.Should().Be("Mensaje generado");
        message.Intent.Should().Be("humor seco");
        _factory.Llm.Verify(l => l.GenerateMessageAsync("humor seco", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Generate_without_a_body_uses_the_default_intent_of_the_user()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/messages/generate", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var message = (await response.Content.ReadFromJsonAsync<DailyMessageDto>())!;
        message.Intent.Should().Be(new User().DefaultIntent);
    }

    [Fact]
    public async Task Today_generates_once_and_then_returns_the_same_message()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var first = await client.GetFromJsonAsync<DailyMessageDto>("/api/messages/today");
        var second = await client.GetFromJsonAsync<DailyMessageDto>("/api/messages/today");

        second.Should().Be(first);
        _factory.Llm.Verify(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task History_returns_only_the_requested_page_of_the_users_messages()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        for (var i = 0; i < 3; i++)
        {
            (await client.PostAsync("/api/messages/generate", content: null)).EnsureSuccessStatusCode();
        }

        var page = await client.GetFromJsonAsync<PagedResult<DailyMessageDto>>("/api/messages/history?page=1&size=2");

        page!.Items.Should().HaveCount(2);
        page.Page.Should().Be(1);
        page.Size.Should().Be(2);
        page.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Messages_read_back_from_the_database_serialize_created_at_as_utc()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        (await client.PostAsync("/api/messages/generate", content: null)).EnsureSuccessStatusCode();

        var today = await client.GetFromJsonAsync<JsonElement>("/api/messages/today");
        var history = await client.GetFromJsonAsync<JsonElement>("/api/messages/history?page=1&size=1");

        today.GetProperty("createdAt").GetString().Should().EndWith("Z");
        history.GetProperty("items")[0].GetProperty("createdAt").GetString().Should().EndWith("Z");
    }

    [Fact]
    public async Task History_with_a_size_out_of_range_returns_400_problem_details()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/messages/history?page=1&size={MessageRules.MaxPageSize + 1}");

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").TryGetProperty("Size", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Generate_returns_503_problem_details_when_the_llm_is_unavailable()
    {
        _factory.Llm.Setup(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LlmUnavailableException("LM Studio could not be reached."));
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/messages/generate", content: null);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.ServiceUnavailable);
        problem.GetProperty("detail").GetString().Should().Be("LM Studio could not be reached.");
    }

    [Fact]
    public async Task Today_returns_404_problem_details_when_the_token_user_no_longer_exists()
    {
        using var client = _factory.CreateClient().WithBearer(TokenFor(new User { Id = Guid.NewGuid(), Email = "ghost@test.dev" }));

        var response = await client.GetAsync("/api/messages/today");

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Unexpected_errors_return_500_problem_details_without_internal_details()
    {
        _factory.Llm.Setup(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("internal-secret-detail"));
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/messages/generate", content: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.InternalServerError);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("internal-secret-detail");
    }

    private string TokenFor(User user)
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateToken(user);
    }
}
