using DailyVibe.Application.Auth;
using DailyVibe.Application.Messages;
using DailyVibe.Application.Preferences;
using FluentAssertions;
using FluentValidation;

namespace DailyVibe.Tests.Application;

public sealed class ValidatorTests
{
    private const string ValidEmail = "ana@test.dev";
    private const string ValidPassword = "s3cret-password";
    private static readonly Guid UserId = Guid.NewGuid();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("ana@")]
    [InlineData("@test.dev")]
    public void Register_rejects_an_invalid_email(string email) =>
        ErrorsOf(new RegisterUserCommandValidator(), new RegisterUserCommand(email, ValidPassword))
            .Should().Contain(nameof(RegisterUserCommand.Email));

    [Fact]
    public void Register_rejects_an_email_longer_than_the_column() =>
        ErrorsOf(new RegisterUserCommandValidator(), new RegisterUserCommand(new string('a', 250) + "@test.dev", ValidPassword))
            .Should().Contain(nameof(RegisterUserCommand.Email));

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("1234567")]
    public void Register_rejects_a_short_password(string password) =>
        ErrorsOf(new RegisterUserCommandValidator(), new RegisterUserCommand(ValidEmail, password))
            .Should().Contain(nameof(RegisterUserCommand.Password));

    [Fact]
    public void Register_rejects_a_password_that_bcrypt_would_truncate() =>
        ErrorsOf(new RegisterUserCommandValidator(), new RegisterUserCommand(ValidEmail, new string('ñ', 37)))
            .Should().Contain(nameof(RegisterUserCommand.Password));

    [Fact]
    public void Register_accepts_valid_credentials() =>
        ErrorsOf(new RegisterUserCommandValidator(), new RegisterUserCommand(ValidEmail, "12345678")).Should().BeEmpty();

    [Theory]
    [InlineData("not-an-email", ValidPassword, nameof(LoginCommand.Email))]
    [InlineData(ValidEmail, "", nameof(LoginCommand.Password))]
    public void Login_rejects_invalid_input(string email, string password, string property) =>
        ErrorsOf(new LoginCommandValidator(), new LoginCommand(email, password)).Should().Contain(property);

    [Fact]
    public void Generate_rejects_an_intent_longer_than_500_characters() =>
        ErrorsOf(new GenerateMessageCommandValidator(), new GenerateMessageCommand(UserId, new string('x', 501)))
            .Should().Contain(nameof(GenerateMessageCommand.Intent));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("motivacional")]
    public void Generate_accepts_a_missing_or_short_intent(string? intent) =>
        ErrorsOf(new GenerateMessageCommandValidator(), new GenerateMessageCommand(UserId, intent)).Should().BeEmpty();

    [Fact]
    public void Generate_accepts_an_intent_of_exactly_500_characters() =>
        ErrorsOf(new GenerateMessageCommandValidator(), new GenerateMessageCommand(UserId, new string('x', 500))).Should().BeEmpty();

    [Fact]
    public void Generate_rejects_an_empty_user_id() =>
        ErrorsOf(new GenerateMessageCommandValidator(), new GenerateMessageCommand(Guid.Empty, "x"))
            .Should().Contain(nameof(GenerateMessageCommand.UserId));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Preferences_rejects_a_blank_intent(string intent) =>
        ErrorsOf(new UpdatePreferencesCommandValidator(), new UpdatePreferencesCommand(UserId, intent))
            .Should().Contain(nameof(UpdatePreferencesCommand.DefaultIntent));

    [Fact]
    public void Preferences_rejects_an_intent_longer_than_500_characters() =>
        ErrorsOf(new UpdatePreferencesCommandValidator(), new UpdatePreferencesCommand(UserId, new string('x', 501)))
            .Should().Contain(nameof(UpdatePreferencesCommand.DefaultIntent));

    [Theory]
    [InlineData(0, 10, nameof(GetMessageHistoryQuery.Page))]
    [InlineData(1, 0, nameof(GetMessageHistoryQuery.Size))]
    [InlineData(1, 51, nameof(GetMessageHistoryQuery.Size))]
    public void History_rejects_out_of_range_paging(int page, int size, string property) =>
        ErrorsOf(new GetMessageHistoryQueryValidator(), new GetMessageHistoryQuery(UserId, page, size)).Should().Contain(property);

    [Fact]
    public void History_accepts_default_paging() =>
        ErrorsOf(new GetMessageHistoryQueryValidator(), new GetMessageHistoryQuery(UserId)).Should().BeEmpty();

    private static IEnumerable<string> ErrorsOf<T>(IValidator<T> validator, T request) =>
        validator.Validate(request).Errors.Select(e => e.PropertyName).Distinct();
}
