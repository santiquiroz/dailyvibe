using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Hosting;

namespace DailyVibe.Tests.Api;

public sealed class HostEnvironmentTests(DailyVibeApiFactory factory) : IClassFixture<DailyVibeApiFactory>
{
    private const string DevClientOrigin = "http://localhost:4200";

    [Fact]
    public async Task Development_allows_preflight_requests_from_the_angular_dev_client()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight(DevClientOrigin));

        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Equal(DevClientOrigin);
    }

    [Fact]
    public async Task Development_rejects_preflight_requests_from_other_origins()
    {
        using var client = factory.CreateClient();

        var response = await client.SendAsync(Preflight("http://evil.example"));

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    [Fact]
    public async Task Production_does_not_expose_swagger()
    {
        using var production = new DailyVibeApiFactory { EnvironmentName = Environments.Production };
        using var client = production.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Production_does_not_enable_the_dev_client_cors_policy()
    {
        using var production = new DailyVibeApiFactory { EnvironmentName = Environments.Production };
        using var client = production.CreateClient();

        var response = await client.SendAsync(Preflight(DevClientOrigin));

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    private static HttpRequestMessage Preflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/messages/today");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", "authorization");
        return request;
    }
}
