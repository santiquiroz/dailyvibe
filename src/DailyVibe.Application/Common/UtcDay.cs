namespace DailyVibe.Application.Common;

public static class UtcDay
{
    public static (DateTime Start, DateTime End) Containing(DateTimeOffset instant)
    {
        var start = instant.UtcDateTime.Date;
        return (start, start.AddDays(1));
    }
}
