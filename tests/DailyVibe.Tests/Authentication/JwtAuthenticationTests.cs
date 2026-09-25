using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DailyVibe.Tests.Authentication;

public sealed class JwtAuthenticationTests(JwtApiFactory factory) : IClassFixture<JwtApiFactory>
{
    private const string ProtectedPath = "/test/protected";

    private static readonly User TestUser = new() { Id = Guid.NewGuid(), Email = "ana@example.com" };

    [Fact]
    public void Token_from_JwtService_validates_with_registered_bearer_parameters()
    {
        var token = GenerateToken(TestUser);
        var parameters = factory.Services
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme)
            .TokenValidationParameters;

        new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var validated);

        var jwt = validated.Should().BeOfType<JwtSecurityToken>().Subject;
        jwt.Subject.Should().Be(TestUser.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == TestUser.Email);
    }

    [Fact]
    public async Task Protected_endpoint_returns_401_without_token()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(ProtectedPath);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Protected_endpoint_returns_200_with_valid_token()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken(TestUser));

        var response = await client.GetAsync(ProtectedPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("only-16-chars-!!")]
    public void Host_fails_to_start_when_jwt_secret_is_empty_or_short(string secret)
    {
        using var invalidFactory = new JwtApiFactory { JwtSecret = secret };

        var startHost = () => invalidFactory.Services;

        startHost.Should().Throw<OptionsValidationException>();
    }

    private string GenerateToken(User user)
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateToken(user);
    }
}
