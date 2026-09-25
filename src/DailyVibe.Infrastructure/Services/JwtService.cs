using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using DailyVibe.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DailyVibe.Infrastructure.Services;

public class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    public string GenerateToken(User user)
    {
        var jwt = options.Value;
        var credentials = new SigningCredentials(JwtTokenParameters.CreateSigningKey(jwt), JwtTokenParameters.Algorithm);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
