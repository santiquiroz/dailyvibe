using DailyVibe.Api.Persistence;
using DailyVibe.Application.Interfaces;
using DailyVibe.Tests.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

namespace DailyVibe.Tests.Api;

public sealed class DailyVibeApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        $"Data Source=dailyvibe-api-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

    // A shared-cache in-memory SQLite database lives only while at least one connection stays open.
    private readonly SqliteConnection _keepAlive;

    public DailyVibeApiFactory()
    {
        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();
    }

    public string EnvironmentName { get; init; } = Environments.Development;

    public Mock<ILmStudioClient> Llm { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(TestSettings()));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ILmStudioClient>();
            services.AddSingleton(Llm.Object);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive.Dispose();
        }
    }

    private Dictionary<string, string?> TestSettings() => new()
    {
        ["Jwt:Secret"] = JwtApiFactory.ValidSecret,
        ["ConnectionStrings:Default"] = _connectionString,
        [DatabaseMigration.MigrateOnStartupKey] = "true",
    };
}
