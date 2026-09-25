using DailyVibe.Infrastructure.LmStudio;
using FluentAssertions;

namespace DailyVibe.Tests.LmStudio;

public sealed class LmStudioOptionsValidatorTests
{
    private static readonly LmStudioOptions ValidOptions = new()
    {
        BaseUrl = "http://localhost:1234",
        Model = "test/model",
        TimeoutSeconds = 60,
        MaxTokens = 150,
        Temperature = 0.7,
    };

    [Fact]
    public void Accepts_complete_options()
    {
        var result = new LmStudioOptionsValidator().Validate(null, ValidOptions);

        result.Succeeded.Should().BeTrue();
    }

    public static TheoryData<LmStudioOptions, string> InvalidOptions => new()
    {
        { Copy(baseUrl: "localhost:1234"), "LmStudio:BaseUrl" },
        { Copy(baseUrl: "ftp://localhost"), "LmStudio:BaseUrl" },
        { Copy(model: " "), "LmStudio:Model" },
        { Copy(timeoutSeconds: 0), "LmStudio:TimeoutSeconds" },
        { Copy(maxTokens: 0), "LmStudio:MaxTokens" },
        { Copy(temperature: -0.1), "LmStudio:Temperature" },
        { Copy(temperature: LmStudioOptionsValidator.MaxTemperature + 0.1), "LmStudio:Temperature" },
        { Copy(temperature: double.NaN), "LmStudio:Temperature" },
    };

    [Theory]
    [MemberData(nameof(InvalidOptions))]
    public void Rejects_invalid_options_naming_the_offending_key(LmStudioOptions options, string expectedKey)
    {
        var result = new LmStudioOptionsValidator().Validate(null, options);

        result.Failed.Should().BeTrue();
        result.Failures.Should().ContainSingle().Which.Should().StartWith(expectedKey);
    }

    private static LmStudioOptions Copy(
        string? baseUrl = null, string? model = null, int? timeoutSeconds = null,
        int? maxTokens = null, double? temperature = null) => new()
    {
        BaseUrl = baseUrl ?? ValidOptions.BaseUrl,
        Model = model ?? ValidOptions.Model,
        TimeoutSeconds = timeoutSeconds ?? ValidOptions.TimeoutSeconds,
        MaxTokens = maxTokens ?? ValidOptions.MaxTokens,
        Temperature = temperature ?? ValidOptions.Temperature,
    };
}
