using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DailyVibe.Infrastructure.Persistence.Configurations;

// SQLite stores DateTime without a Kind, so values would come back Unspecified and serialize without an offset.
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value,
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
