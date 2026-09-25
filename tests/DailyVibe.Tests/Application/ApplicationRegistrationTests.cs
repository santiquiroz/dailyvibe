using DailyVibe.Application;
using DailyVibe.Application.Interfaces;
using DailyVibe.Infrastructure;
using DailyVibe.Infrastructure.Services;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DailyVibe.Tests.Application;

public sealed class ApplicationRegistrationTests
{
    [Fact]
    public void Application_and_infrastructure_resolve_every_handler_dependency()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["LmStudio:BaseUrl"] = "http://lmstudio.test",
            ["LmStudio:Model"] = "test/model",
        }).Build();
        var services = new ServiceCollection().AddLogging().AddApplication().AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ISender>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Should().BeOfType<BcryptPasswordHasher>();
        scope.ServiceProvider.GetRequiredService<IUserRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IDailyMessageRepository>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
    }
}
