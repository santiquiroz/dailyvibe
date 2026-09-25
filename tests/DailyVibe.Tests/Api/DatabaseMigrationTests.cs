using DailyVibe.Api.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DailyVibe.Tests.Api;

public sealed class DatabaseMigrationTests
{
    [Theory]
    [InlineData(null, true, true)]
    [InlineData(null, false, false)]
    [InlineData("false", true, false)]
    [InlineData("true", false, true)]
    public void Migrates_in_development_unless_the_flag_says_otherwise(string? flag, bool isDevelopment, bool expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [DatabaseMigration.MigrateOnStartupKey] = flag })
            .Build();

        DatabaseMigration.ShouldMigrate(configuration, isDevelopment).Should().Be(expected);
    }
}
