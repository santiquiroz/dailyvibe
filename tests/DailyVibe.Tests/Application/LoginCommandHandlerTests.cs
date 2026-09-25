using DailyVibe.Application.Auth;
using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class LoginCommandHandlerTests
{
    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "ana@test.dev", PasswordHash = "hash" };
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();

    public LoginCommandHandlerTests()
    {
        _users.Setup(u => u.FindByEmailAsync(_user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _hasher.Setup(h => h.Verify("correct-password", "hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(_user)).Returns("jwt-token");
    }

    [Fact]
    public async Task Throws_authentication_failed_for_a_wrong_password()
    {
        var act = () => CreateHandler().Handle(new LoginCommand(_user.Email, "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
        _jwt.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Throws_authentication_failed_for_an_unknown_email()
    {
        var act = () => CreateHandler().Handle(new LoginCommand("nadie@test.dev", "correct-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task Returns_a_token_for_valid_credentials_matching_the_email_case_insensitively()
    {
        var result = await CreateHandler().Handle(new LoginCommand("  Ana@Test.DEV ", "correct-password"), CancellationToken.None);

        result.Should().Be(new AuthResult(_user.Id, _user.Email, "jwt-token"));
    }

    private LoginCommandHandler CreateHandler() => new(_users.Object, _hasher.Object, _jwt.Object);
}
