using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DailyVibe.Tests.Authentication;

public sealed class JwtApiFactory : WebApplicationFactory<Program>
{
    public const string ValidSecret = "integration-tests-secret-at-least-32-bytes!";

    public string JwtSecret { get; init; } = ValidSecret;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(TestSettings()));
        builder.ConfigureTestServices(services =>
            services.AddControllers().AddApplicationPart(typeof(ProtectedTestController).Assembly));
    }

    private Dictionary<string, string?> TestSettings() => new()
    {
        ["Jwt:Secret"] = JwtSecret,
        ["ConnectionStrings:Default"] = "Data Source=:memory:",
    };
}
