using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace DailyVibe.Tests.Api;

public sealed class OpenApiDocumentTests(DailyVibeApiFactory factory) : IClassFixture<DailyVibeApiFactory>
{
    private const string DocumentPath = "/swagger/v1/swagger.json";

    [Theory]
    [InlineData("/api/auth/register", "post")]
    [InlineData("/api/auth/login", "post")]
    [InlineData("/api/messages/today", "get")]
    [InlineData("/api/messages/generate", "post")]
    [InlineData("/api/messages/history", "get")]
    [InlineData("/api/preferences", "put")]
    public async Task Document_lists_every_api_route(string path, string method)
    {
        var document = await GetDocumentAsync();

        document.GetProperty("paths").GetProperty(path).TryGetProperty(method, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Document_declares_the_bearer_security_scheme()
    {
        var document = await GetDocumentAsync();

        var bearer = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        bearer.GetProperty("type").GetString().Should().Be("http");
        bearer.GetProperty("scheme").GetString().Should().Be("bearer");
    }

    [Theory]
    [InlineData("/api/messages/today", "get")]
    [InlineData("/api/messages/generate", "post")]
    [InlineData("/api/messages/history", "get")]
    [InlineData("/api/preferences", "put")]
    public async Task Protected_operations_require_the_bearer_scheme(string path, string method)
    {
        var operation = (await GetDocumentAsync()).GetProperty("paths").GetProperty(path).GetProperty(method);

        operation.GetProperty("security").EnumerateArray()
            .Should().ContainSingle(requirement => requirement.EnumerateObject().Any(scheme => scheme.Name == "Bearer"));
    }

    [Theory]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/login")]
    public async Task Anonymous_operations_do_not_require_a_token(string path)
    {
        var operation = (await GetDocumentAsync()).GetProperty("paths").GetProperty(path).GetProperty("post");

        operation.TryGetProperty("security", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Swagger_ui_is_served_in_development()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<JsonElement> GetDocumentAsync()
    {
        using var client = factory.CreateClient();
        var json = await client.GetStringAsync(DocumentPath);
        return JsonDocument.Parse(json).RootElement;
    }
}
