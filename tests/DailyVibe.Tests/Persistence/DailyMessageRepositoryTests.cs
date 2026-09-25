using DailyVibe.Domain.Entities;
using DailyVibe.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace DailyVibe.Tests.Persistence;

public sealed class DailyMessageRepositoryTests : IDisposable
{
    private static readonly DateTime Day = new(2026, 9, 25, 0, 0, 0, DateTimeKind.Utc);

    private readonly SqliteInMemoryDatabase _database = new();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public DailyMessageRepositoryTests()
    {
        using var context = _database.CreateContext();
        context.Users.AddRange(NewUser(_userId, "ana@test.dev"), NewUser(_otherUserId, "otro@test.dev"));
        context.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Finds_the_latest_message_of_the_user_inside_the_range()
    {
        await AddAsync(
            NewMessage(_userId, "ayer", Day.AddMinutes(-1)),
            NewMessage(_userId, "temprano", Day.AddHours(1)),
            NewMessage(_userId, "tarde", Day.AddHours(20)),
            NewMessage(_userId, "mañana", Day.AddDays(1)),
            NewMessage(_otherUserId, "de otro", Day.AddHours(22)));
        await using var context = _database.CreateContext();

        var found = await new DailyMessageRepository(context)
            .FindLatestCreatedBetweenAsync(_userId, Day, Day.AddDays(1), CancellationToken.None);

        found!.Content.Should().Be("tarde");
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task Returns_null_when_the_user_has_no_message_inside_the_range()
    {
        await AddAsync(NewMessage(_userId, "ayer", Day.AddHours(-3)), NewMessage(_otherUserId, "de otro", Day.AddHours(2)));
        await using var context = _database.CreateContext();

        var found = await new DailyMessageRepository(context)
            .FindLatestCreatedBetweenAsync(_userId, Day, Day.AddDays(1), CancellationToken.None);

        found.Should().BeNull();
    }

    [Fact]
    public async Task Pages_the_user_history_newest_first_without_tracking()
    {
        var own = Enumerable.Range(1, 5).Select(i => NewMessage(_userId, $"m{i}", Day.AddHours(i))).ToArray();
        await AddAsync([.. own, NewMessage(_otherUserId, "de otro", Day.AddHours(9))]);
        await using var context = _database.CreateContext();

        var page = await new DailyMessageRepository(context).GetPageAsync(_userId, 2, 2, CancellationToken.None);

        page.Items.Select(m => m.Content).Should().Equal("m3", "m2");
        page.Page.Should().Be(2);
        page.Size.Should().Be(2);
        page.TotalCount.Should().Be(5);
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task Reads_created_at_back_as_utc()
    {
        await AddAsync(NewMessage(_userId, "hoy", Day.AddHours(3)));
        await using var context = _database.CreateContext();

        var page = await new DailyMessageRepository(context).GetPageAsync(_userId, 1, 1, CancellationToken.None);

        page.Items.Single().CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    private async Task AddAsync(params DailyMessage[] messages)
    {
        foreach (var message in messages)
        {
            await using var context = _database.CreateContext();
            await new DailyMessageRepository(context).AddAsync(message, CancellationToken.None);
        }
    }

    private static DailyMessage NewMessage(Guid userId, string content, DateTime createdAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Content = content,
        Intent = "x",
        CreatedAt = createdAt,
    };

    private static User NewUser(Guid id, string email) => new()
    {
        Id = id,
        Email = email,
        PasswordHash = "hash",
        CreatedAt = Day,
    };
}
