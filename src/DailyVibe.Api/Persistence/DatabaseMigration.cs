using DailyVibe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyVibe.Api.Persistence;

public static class DatabaseMigration
{
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";

    public static bool ShouldMigrate(IConfiguration configuration, bool isDevelopment) =>
        configuration.GetValue(MigrateOnStartupKey, isDevelopment);

    public static void MigrateDatabaseIfEnabled(this WebApplication app)
    {
        if (!ShouldMigrate(app.Configuration, app.Environment.IsDevelopment()))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}
