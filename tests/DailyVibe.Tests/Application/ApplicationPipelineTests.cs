using DailyVibe.Application;
using DailyVibe.Application.Auth;
using DailyVibe.Application.Interfaces;
using DailyVibe.Application.Messages;
using DailyVibe.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DailyVibe.Tests.Application;

public sealed class ApplicationPipelineTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 9, 25, 23, 59, 0, TimeSpan.Zero);
    private static readonly DateTime TodayStart = new(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime TomorrowStart = TodayStart.AddDays(1);

    private readonly User _user = new() { Id = Guid.NewGuid(), Email = "ana@test.dev", DefaultIntent = "estoico" };
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IDailyMessageRepository> _messages = new();
    private readonly Mock<ILmStudioClient> _llm = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly ServiceProvider _provider;

    public ApplicationPipelineTests()
    {
        _users.Setup(u => u.FindByIdAsync(_user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_user);
        _provider = BuildProvider();
    }

    public void Dispose() => _provider.Dispose();

    [Fact]
    public async Task Rejects_an_invalid_command_before_its_handler_runs()
    {
        var act = () => Send(new RegisterUserCommand("not-an-email", "short"));

        await act.Should().ThrowAsync<ValidationException>();
        _users.Invocations.Should().BeEmpty();
        _hasher.Invocations.Should().BeEmpty();
    }

    [Fact]
    public async Task Today_returns_the_existing_message_of_the_utc_day_without_calling_the_llm()
    {
        var existing = new DailyMessage
        {
            Id = Guid.NewGuid(),
            UserId = _user.Id,
            Content = "Ya generado",
            Intent = "estoico",
            CreatedAt = TodayStart.AddHours(7),
        };
        _messages.Setup(m => m.FindLatestCreatedBetweenAsync(_user.Id, TodayStart, TomorrowStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await Send(new GetTodayMessageQuery(_user.Id));

        result.Should().Be(new DailyMessageDto(existing.Id, "Ya generado", "estoico", existing.CreatedAt));
        _llm.Invocations.Should().BeEmpty();
        _messages.Verify(m => m.AddAsync(It.IsAny<DailyMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Today_generates_and_persists_a_message_with_the_default_intent_when_none_exists_for_the_day()
    {
        DailyMessage? saved = null;
        _messages.Setup(m => m.AddAsync(It.IsAny<DailyMessage>(), It.IsAny<CancellationToken>()))
            .Callback<DailyMessage, CancellationToken>((message, _) => saved = message)
            .Returns(Task.CompletedTask);
        _llm.Setup(l => l.GenerateMessageAsync("estoico", It.IsAny<CancellationToken>())).ReturnsAsync("Nuevo mensaje");

        var result = await Send(new GetTodayMessageQuery(_user.Id));

        _messages.Verify(m => m.FindLatestCreatedBetweenAsync(_user.Id, TodayStart, TomorrowStart, It.IsAny<CancellationToken>()), Times.Once);
        _llm.Verify(l => l.GenerateMessageAsync("estoico", It.IsAny<CancellationToken>()), Times.Once);
        saved.Should().NotBeNull();
        saved!.CreatedAt.Should().Be(Now.UtcDateTime);
        result.Should().Be(new DailyMessageDto(saved.Id, "Nuevo mensaje", "estoico", Now.UtcDateTime));
    }

    private async Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _provider.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(request);
    }

    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddSingleton<TimeProvider>(new FixedTimeProvider(Now))
            .AddSingleton(_users.Object)
            .AddSingleton(_messages.Object)
            .AddSingleton(_llm.Object)
            .AddSingleton(_hasher.Object)
            .AddSingleton(Mock.Of<IJwtService>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
