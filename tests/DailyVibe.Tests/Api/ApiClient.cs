using System.Net.Http.Headers;
using System.Net.Http.Json;
using DailyVibe.Application.Auth;

namespace DailyVibe.Tests.Api;

public static class ApiClient
{
    public const string Password = "correct-horse-battery";
    public const string ProblemJson = "application/problem+json";

    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.dev";

    public static async Task<AuthResult> RegisterAsync(this HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password = Password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResult>())!;
    }

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this DailyVibeApiFactory factory)
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterAsync(UniqueEmail());
        return client.WithBearer(auth.Token);
    }

    public static HttpClient WithBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
