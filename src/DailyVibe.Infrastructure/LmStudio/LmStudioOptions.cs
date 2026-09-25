namespace DailyVibe.Infrastructure.LmStudio;

public sealed class LmStudioOptions
{
    public const string SectionName = "LmStudio";

    public string BaseUrl { get; init; } = "http://localhost:1234";
    public string Model { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 60;
    public int MaxTokens { get; init; } = 150;
    public double Temperature { get; init; } = 0.7;
}
