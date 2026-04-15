using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DailyVibe.Infrastructure.Persistence;

// Used by 'dotnet ef migrations add' at design time — not used at runtime.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=dailyvibe_design.db")
            .Options;
        return new AppDbContext(options);
    }
}
