using DailyVibe.Infrastructure.Services;
using FluentAssertions;

namespace DailyVibe.Tests.Authentication;

public sealed class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Produces_a_salted_bcrypt_hash_that_verifies_only_the_original_password()
    {
        var hash = _hasher.Hash("s3cret-password");

        hash.Should().StartWith("$2").And.NotContain("s3cret-password");
        _hasher.Hash("s3cret-password").Should().NotBe(hash);
        _hasher.Verify("s3cret-password", hash).Should().BeTrue();
        _hasher.Verify("wrong-password", hash).Should().BeFalse();
    }
}
