using DailyVibe.Application.Auth;
using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class RegisterUserCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly List<User> _persisted = [];

    public RegisterUserCommandHandlerTests()
    {
        _users.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => _persisted.Add(user))
            .Returns(Task.CompletedTask);
        _hasher.Setup(h => h.Hash("s3cret-password")).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");
    }

    [Fact]
    public async Task Persists_the_normalized_email_and_hashed_password_and_returns_a_token()
    {
        var result = await CreateHandler().Handle(new RegisterUserCommand(" Ana@Test.DEV ", "s3cret-password"), CancellationToken.None);

        var saved = _persisted.Should().ContainSingle().Subject;
        saved.Email.Should().Be("ana@test.dev");
        saved.PasswordHash.Should().Be("bcrypt-hash");
        saved.CreatedAt.Should().Be(Now.UtcDateTime);
        saved.Id.Should().NotBeEmpty();
        result.Should().Be(new AuthResult(saved.Id, "ana@test.dev", "jwt-token"));
    }

    [Fact]
    public async Task Rejects_an_email_that_is_already_registered()
    {
        _users.Setup(u => u.EmailExistsAsync("ana@test.dev", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => CreateHandler().Handle(new RegisterUserCommand("ANA@test.dev", "s3cret-password"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyRegisteredException>();
        _persisted.Should().BeEmpty();
    }

    private RegisterUserCommandHandler CreateHandler() =>
        new(_users.Object, _hasher.Object, _jwt.Object, new FixedTimeProvider(Now));
}
