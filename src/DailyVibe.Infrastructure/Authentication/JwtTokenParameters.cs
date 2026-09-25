using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DailyVibe.Infrastructure.Authentication;

public static class JwtTokenParameters
{
    public const string Algorithm = SecurityAlgorithms.HmacSha256;

    public static SymmetricSecurityKey CreateSigningKey(JwtOptions options) =>
        new(Encoding.UTF8.GetBytes(options.Secret));

    public static TokenValidationParameters CreateValidationParameters(JwtOptions options) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = options.Issuer,
        ValidateAudience = true,
        ValidAudience = options.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = CreateSigningKey(options),
        ValidAlgorithms = [Algorithm],
    };
}
