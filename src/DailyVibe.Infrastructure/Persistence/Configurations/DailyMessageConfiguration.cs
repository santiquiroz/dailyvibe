using DailyVibe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyVibe.Infrastructure.Persistence.Configurations;

public class DailyMessageConfiguration : IEntityTypeConfiguration<DailyMessage>
{
    public void Configure(EntityTypeBuilder<DailyMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.Intent)
            .IsRequired()
            .HasMaxLength(500);
    }
}
