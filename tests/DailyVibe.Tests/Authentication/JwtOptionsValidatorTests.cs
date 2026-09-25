using DailyVibe.Infrastructure.Authentication;
using FluentAssertions;

namespace DailyVibe.Tests.Authentication;

public sealed class JwtOptionsValidatorTests
{
    private static readonly JwtOptions ValidOptions = new()
    {
        Secret = new string('s', JwtOptionsValidator.MinSecretBytes),
        Issuer = "dailyvibe-api",
        Audience = "dailyvibe-client",
        ExpiryMinutes = 60,
    };

    [Fact]
    public void Accepts_complete_options_with_32_byte_secret()
    {
        var result = new JwtOptionsValidator().Validate(null, ValidOptions);

        result.Succeeded.Should().BeTrue();
    }

    public static TheoryData<JwtOptions, string> InvalidOptions => new()
    {
        { Copy(secret: ""), "Jwt:Secret" },
        { Copy(secret: new string('s', JwtOptionsValidator.MinSecretBytes - 1)), "Jwt:Secret" },
        { Copy(issuer: " "), "Jwt:Issuer" },
        { Copy(audience: ""), "Jwt:Audience" },
        { Copy(expiryMinutes: 0), "Jwt:ExpiryMinutes" },
    };

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void Rejects_invalid_options_naming_the_offending_key(JwtOptions options, string expectedKey)
    {
        var result = new JwtOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().StartWith(expectedKey);
    }

    private static JwtOptions Copy(
        string? secret = null, string? issuer = null, string? audience = null, int? expiryMinutes = null) => new()
    {
        Secret = secret ?? ValidOptions.Secret,
        Issuer = issuer ?? ValidOptions.Issuer,
        Audience = audience ?? ValidOptions.Audience,
        ExpiryMinutes = expiryMinutes ?? ValidOptions.ExpiryMinutes,
    };
}
