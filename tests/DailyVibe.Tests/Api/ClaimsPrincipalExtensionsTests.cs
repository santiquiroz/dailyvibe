using System.Security.Claims;
using DailyVibe.Api.Authentication;
using FluentAssertions;

namespace DailyVibe.Tests.Api;

public sealed class ClaimsPrincipalExtensionsTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Theory]
    [InlineData(ClaimTypes.NameIdentifier)]
    [InlineData("sub")]
    public void Reads_the_user_id_from_the_subject_claim(string claimType)
    {
        var principal = PrincipalWith(new Claim(claimType, UserId.ToString()));

        principal.GetUserId().Should().Be(UserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public void Throws_when_the_subject_is_missing_or_not_a_guid(string? subject)
    {
        Claim[] claims = subject is null ? [] : [new Claim(ClaimTypes.NameIdentifier, subject)];
        var principal = PrincipalWith(claims);

        var act = () => principal.GetUserId();

        act.Should().Throw<MissingUserIdentityException>();
    }

    private static ClaimsPrincipal PrincipalWith(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Bearer"));
}
