using Microsoft.Extensions.Options;

namespace DailyVibe.Infrastructure.LmStudio;

public sealed class LmStudioOptionsValidator : IValidateOptions<LmStudioOptions>
{
    public const double MaxTemperature = 2.0;

    public ValidateOptionsResult Validate(string? name, LmStudioOptions options)
    {
        var failures = CollectFailures(options).ToList();
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static IEnumerable<string> CollectFailures(LmStudioOptions options)
    {
        if (!IsHttpUrl(options.BaseUrl))
            yield return "LmStudio:BaseUrl must be an absolute http(s) URL.";
        if (string.IsNullOrWhiteSpace(options.Model))
            yield return "LmStudio:Model is required.";
        if (options.TimeoutSeconds <= 0)
            yield return "LmStudio:TimeoutSeconds must be greater than zero.";
        if (options.MaxTokens <= 0)
            yield return "LmStudio:MaxTokens must be greater than zero.";
        if (options.Temperature is not (>= 0 and <= MaxTemperature))
            yield return $"LmStudio:Temperature must be between 0 and {MaxTemperature}.";
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
