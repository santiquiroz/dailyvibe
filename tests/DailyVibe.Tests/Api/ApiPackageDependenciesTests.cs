using System.Text.Json;
using FluentAssertions;

namespace DailyVibe.Tests.Api;

public sealed class ApiPackageDependenciesTests
{
    private const int MinimumFluentValidationMajor = 12;

    [Fact]
    public void Api_does_not_depend_on_the_deprecated_FluentValidation_AspNetCore_integration()
    {
        var packages = ReadApiPackages();

        packages.Select(p => p.Name).Should().NotContain("FluentValidation.AspNetCore");
    }

    [Fact]
    public void Api_resolves_every_FluentValidation_package_at_the_supported_major()
    {
        var fluentValidationPackages = ReadApiPackages()
            .Where(p => p.Name.StartsWith("FluentValidation", StringComparison.Ordinal))
            .ToList();

        fluentValidationPackages.Should().NotBeEmpty();
        fluentValidationPackages.Should().OnlyContain(p => p.Version.Major >= MinimumFluentValidationMajor);
    }

    private static IReadOnlyList<(string Name, Version Version)> ReadApiPackages()
    {
        var depsPath = Path.Combine(AppContext.BaseDirectory, "DailyVibe.Api.deps.json");
        using var deps = JsonDocument.Parse(File.ReadAllText(depsPath));

        return deps.RootElement.GetProperty("libraries")
            .EnumerateObject()
            .Where(library => library.Value.GetProperty("type").GetString() == "package")
            .Select(library => ParseLibraryKey(library.Name))
            .ToList();
    }

    private static (string Name, Version Version) ParseLibraryKey(string key)
    {
        var separator = key.LastIndexOf('/');
        return (key[..separator], Version.Parse(key[(separator + 1)..]));
    }
}
