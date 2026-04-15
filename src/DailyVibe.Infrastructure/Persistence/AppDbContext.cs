using DailyVibe.Domain.Entities;
using DailyVibe.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DailyVibe.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DailyMessage> DailyMessages => Set<DailyMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new DailyMessageConfiguration());
    }
}
