namespace DailyVibe.Api.Cors;

public static class DevClientCors
{
    public const string PolicyName = "DevClient";
    public const string Origin = "http://localhost:4200";

    public static IServiceCollection AddDevClientCors(this IServiceCollection services) =>
        services.AddCors(options => options.AddPolicy(PolicyName, policy =>
            policy.WithOrigins(Origin).AllowAnyHeader().AllowAnyMethod()));
}
