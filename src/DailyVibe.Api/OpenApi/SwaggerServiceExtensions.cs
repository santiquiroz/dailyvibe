using Microsoft.OpenApi;

namespace DailyVibe.Api.OpenApi;

public static class SwaggerServiceExtensions
{
    public const string BearerSchemeId = "Bearer";

    public static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "DailyVibe API", Version = "v1" });
            options.AddSecurityDefinition(BearerSchemeId, BearerScheme());
            options.OperationFilter<AuthorizeOperationFilter>();
        });

    private static OpenApiSecurityScheme BearerScheme() => new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT returned by /api/auth/register or /api/auth/login.",
    };
}
