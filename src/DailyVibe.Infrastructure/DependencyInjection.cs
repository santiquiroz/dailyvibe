using DailyVibe.Application.Interfaces;
using DailyVibe.Infrastructure.Persistence;
using DailyVibe.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DailyVibe.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IJwtService, JwtService>();

        services.AddHttpClient<ILmStudioClient, LmStudioHttpClient>(client =>
        {
            var baseUrl = configuration["LmStudio:BaseUrl"] ?? "http://localhost:1234";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        return services;
    }
}
