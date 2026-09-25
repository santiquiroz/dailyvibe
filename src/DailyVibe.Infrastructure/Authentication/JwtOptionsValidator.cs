using System.Text;
using Microsoft.Extensions.Options;

namespace DailyVibe.Infrastructure.Authentication;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    // HS256 requires a key of at least 256 bits.
    public const int MinSecretBytes = 32;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var failures = CollectFailures(options).ToList();
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static IEnumerable<string> CollectFailures(JwtOptions options)
    {
        if (Encoding.UTF8.GetByteCount(options.Secret) < MinSecretBytes)
            yield return $"Jwt:Secret must be at least {MinSecretBytes} bytes.";
        if (string.IsNullOrWhiteSpace(options.Issuer))
            yield return "Jwt:Issuer is required.";
        if (string.IsNullOrWhiteSpace(options.Audience))
            yield return "Jwt:Audience is required.";
        if (options.ExpiryMinutes <= 0)
            yield return "Jwt:ExpiryMinutes must be greater than zero.";
    }
}
