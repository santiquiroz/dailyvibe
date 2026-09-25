using DailyVibe.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace DailyVibe.Api.Authentication;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
                bearer.TokenValidationParameters = JwtTokenParameters.CreateValidationParameters(jwt.Value));
        return services;
    }
}
