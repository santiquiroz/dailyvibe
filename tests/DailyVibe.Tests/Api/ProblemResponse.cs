using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace DailyVibe.Tests.Api;

public static class ProblemResponse
{
    public static async Task<JsonElement> ShouldBeProblemAsync(this HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected);
        response.Content.Headers.ContentType!.MediaType.Should().Be(ApiClient.ProblemJson);
        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        problem.GetProperty("status").GetInt32().Should().Be((int)expected);
        return problem;
    }
}
