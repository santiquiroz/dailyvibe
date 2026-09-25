using DailyVibe.Application.Exceptions;
using DailyVibe.Domain.Entities;
using DailyVibe.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace DailyVibe.Tests.Persistence;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly SqliteInMemoryDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Adds_and_finds_a_user_by_id_and_email_without_tracking()
    {
        var user = NewUser("ana@test.dev");
        await AddAsync(user);

        await using var context = _database.CreateContext();
        var repository = new UserRepository(context);

        (await repository.FindByIdAsync(user.Id, CancellationToken.None))!.Email.Should().Be("ana@test.dev");
        (await repository.FindByEmailAsync("ana@test.dev", CancellationToken.None))!.Id.Should().Be(user.Id);
        (await repository.EmailExistsAsync("ana@test.dev", CancellationToken.None)).Should().BeTrue();
        (await repository.EmailExistsAsync("otro@test.dev", CancellationToken.None)).Should().BeFalse();
        context.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task Translates_a_unique_email_violation_into_email_already_registered()
    {
        await AddAsync(NewUser("ana@test.dev"));

        var act = () => AddAsync(NewUser("ana@test.dev"));

        await act.Should().ThrowAsync<EmailAlreadyRegisteredException>();
    }

    [Fact]
    public async Task Updates_the_default_intent_of_an_existing_user()
    {
        var user = NewUser("ana@test.dev");
        await AddAsync(user);

        await using (var context = _database.CreateContext())
        {
            (await new UserRepository(context).UpdateDefaultIntentAsync(user.Id, "reflexivo", CancellationToken.None)).Should().BeTrue();
        }

        await using var verification = _database.CreateContext();
        (await new UserRepository(verification).FindByIdAsync(user.Id, CancellationToken.None))!.DefaultIntent.Should().Be("reflexivo");
    }

    [Fact]
    public async Task Reports_false_when_updating_the_intent_of_an_unknown_user()
    {
        await using var context = _database.CreateContext();

        var updated = await new UserRepository(context).UpdateDefaultIntentAsync(Guid.NewGuid(), "reflexivo", CancellationToken.None);

        updated.Should().BeFalse();
    }

    [Fact]
    public async Task Reads_created_at_back_as_utc()
    {
        var user = NewUser("ana@test.dev");
        await AddAsync(user);
        await using var context = _database.CreateContext();

        var found = await new UserRepository(context).FindByIdAsync(user.Id, CancellationToken.None);

        found!.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    private async Task AddAsync(User user)
    {
        await using var context = _database.CreateContext();
        await new UserRepository(context).AddAsync(user, CancellationToken.None);
    }

    private static User NewUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = "hash",
        CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
    };
}
