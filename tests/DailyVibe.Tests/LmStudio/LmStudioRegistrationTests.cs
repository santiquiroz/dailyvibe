using DailyVibe.Application.Interfaces;
using DailyVibe.Infrastructure;
using DailyVibe.Infrastructure.LmStudio;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DailyVibe.Tests.LmStudio;

public sealed class LmStudioRegistrationTests
{
    [Fact]
    public void Configures_http_client_base_address_and_timeout_from_LmStudio_section()
    {
        using var provider = BuildProvider(new()
        {
            ["LmStudio:BaseUrl"] = "http://lmstudio.test:4321",
            ["LmStudio:Model"] = "test/model",
            ["LmStudio:TimeoutSeconds"] = "15",
        });

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(ILmStudioClient));

        httpClient.BaseAddress.Should().Be(new Uri("http://lmstudio.test:4321"));
        httpClient.Timeout.Should().Be(TimeSpan.FromSeconds(15));
        provider.GetRequiredService<ILmStudioClient>().Should().NotBeNull();
    }

    [Fact]
    public void Rejects_configuration_without_a_model()
    {
        using var provider = BuildProvider(new() { ["LmStudio:BaseUrl"] = "http://lmstudio.test" });

        var act = () => provider.GetRequiredService<IOptions<LmStudioOptions>>().Value;

        act.Should().Throw<OptionsValidationException>().WithMessage("*LmStudio:Model*");
    }

    private static ServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddInfrastructure(configuration).BuildServiceProvider();
    }
}
