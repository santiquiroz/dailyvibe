namespace DailyVibe.Application.Messages;

public static class MessageRules
{
    public const int IntentMaxLength = 500;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    public static string ResolveIntent(string? requested, string fallback) =>
        string.IsNullOrWhiteSpace(requested) ? fallback : requested.Trim();
}
