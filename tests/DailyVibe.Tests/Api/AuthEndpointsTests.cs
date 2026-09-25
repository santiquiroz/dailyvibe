using System.Net;
using System.Net.Http.Json;
using DailyVibe.Application.Auth;
using FluentAssertions;

namespace DailyVibe.Tests.Api;

public sealed class AuthEndpointsTests(DailyVibeApiFactory factory) : IClassFixture<DailyVibeApiFactory>
{
    [Fact]
    public async Task Register_with_an_invalid_email_returns_400_problem_details()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "not-an-email", password = ApiClient.Password });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest);
        problem.GetProperty("errors").TryGetProperty("Email", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Register_returns_a_token_for_the_normalized_email()
    {
        using var client = factory.CreateClient();
        var email = ApiClient.UniqueEmail();

        var result = await client.RegisterAsync($"  {email.ToUpperInvariant()} ");

        result.Email.Should().Be(email);
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_with_an_email_already_taken_returns_409_problem_details()
    {
        using var client = factory.CreateClient();
        var email = ApiClient.UniqueEmail();
        await client.RegisterAsync(email);

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password = ApiClient.Password });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_with_the_registered_credentials_returns_a_token_for_the_same_user()
    {
        using var client = factory.CreateClient();
        var email = ApiClient.UniqueEmail();
        var registered = await client.RegisterAsync(email);

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = ApiClient.Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = (await response.Content.ReadFromJsonAsync<AuthResult>())!;
        login.UserId.Should().Be(registered.UserId);
        login.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_with_a_wrong_password_returns_401_problem_details()
    {
        using var client = factory.CreateClient();
        var email = ApiClient.UniqueEmail();
        await client.RegisterAsync(email);

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "wrong-password" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized);
    }
}
