using DailyVibe.Application.Interfaces;
using DailyVibe.Infrastructure.Authentication;
using DailyVibe.Infrastructure.LmStudio;
using DailyVibe.Infrastructure.Persistence;
using DailyVibe.Infrastructure.Persistence.Repositories;
using DailyVibe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DailyVibe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDailyMessageRepository, DailyMessageRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddOptions<LmStudioOptions>()
            .Bind(configuration.GetSection(LmStudioOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<LmStudioOptions>, LmStudioOptionsValidator>();
        services.AddHttpClient<ILmStudioClient, LmStudioHttpClient>((provider, client) =>
        {
            var lmStudio = provider.GetRequiredService<IOptions<LmStudioOptions>>().Value;
            client.BaseAddress = new Uri(lmStudio.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(lmStudio.TimeoutSeconds);
        });

        return services;
    }
}
