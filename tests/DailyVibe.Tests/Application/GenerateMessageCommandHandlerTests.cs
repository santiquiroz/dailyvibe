using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Application.Messages;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class GenerateMessageCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 14, 30, 0, TimeSpan.Zero);

    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "ana@test.dev", DefaultIntent = "estoico, 1 frase" };
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IDailyMessageRepository> _messages = new();
    private readonly Mock<ILmStudioClient> _llm = new();
    private readonly List<DailyMessage> _persisted = [];

    public GenerateMessageCommandHandlerTests()
    {
        _users.Setup(u => u.FindByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _messages.Setup(m => m.AddAsync(It.IsAny<DailyMessage>(), It.IsAny<CancellationToken>()))
            .Callback<DailyMessage, CancellationToken>((message, _) => _persisted.Add(message))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Persists_exactly_the_content_returned_by_the_llm()
    {
        _llm.Setup(l => l.GenerateMessageAsync("humor técnico", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Compila a la primera: hoy es tu día.");

        var result = await CreateHandler().Handle(new GenerateMessageCommand(_user.Id, "humor técnico"), CancellationToken.None);

        var saved = _persisted.Should().ContainSingle().Subject;
        saved.Content.Should().Be("Compila a la primera: hoy es tu día.");
        saved.Intent.Should().Be("humor técnico");
        saved.UserId.Should().Be(_user.Id);
        saved.CreatedAt.Should().Be(Now.UtcDateTime);
        saved.Id.Should().NotBeEmpty();
        result.Should().Be(new DailyMessageDto(saved.Id, saved.Content, saved.Intent, saved.CreatedAt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Falls_back_to_the_user_default_intent_when_none_is_requested(string? intent)
    {
        _llm.Setup(l => l.GenerateMessageAsync(_user.DefaultIntent, It.IsAny<CancellationToken>())).ReturnsAsync("Respira.");

        await CreateHandler().Handle(new GenerateMessageCommand(_user.Id, intent), CancellationToken.None);

        _persisted.Should().ContainSingle().Which.Intent.Should().Be(_user.DefaultIntent);
    }

    [Fact]
    public async Task Throws_not_found_without_calling_the_llm_when_the_user_does_not_exist()
    {
        var act = () => CreateHandler().Handle(new GenerateMessageCommand(Guid.NewGuid(), "x"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _llm.VerifyNoOtherCalls();
        _persisted.Should().BeEmpty();
    }

    [Fact]
    public async Task Persists_nothing_when_the_llm_is_unavailable()
    {
        _llm.Setup(l => l.GenerateMessageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LlmUnavailableException("down"));

        var act = () => CreateHandler().Handle(new GenerateMessageCommand(_user.Id, "x"), CancellationToken.None);

        await act.Should().ThrowAsync<LlmUnavailableException>();
        _persisted.Should().BeEmpty();
    }

    private GenerateMessageCommandHandler CreateHandler() =>
        new(_users.Object, _messages.Object, _llm.Object, new FixedTimeProvider(Now));
}
