using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Application.Preferences;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class UpdatePreferencesCommandHandlerTests
{
    private readonly Mock<IUserRepository> _users = new();

    [Fact]
    public async Task Stores_the_trimmed_default_intent()
    {
        var userId = Guid.NewGuid();
        _users.Setup(u => u.UpdateDefaultIntentAsync(userId, "reflexivo", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await new UpdatePreferencesCommandHandler(_users.Object)
            .Handle(new UpdatePreferencesCommand(userId, "  reflexivo "), CancellationToken.None);

        _users.Verify(u => u.UpdateDefaultIntentAsync(userId, "reflexivo", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        var act = () => new UpdatePreferencesCommandHandler(_users.Object)
            .Handle(new UpdatePreferencesCommand(Guid.NewGuid(), "reflexivo"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
