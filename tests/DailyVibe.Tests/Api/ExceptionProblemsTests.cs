using DailyVibe.Api.Authentication;
using DailyVibe.Api.ErrorHandling;
using DailyVibe.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace DailyVibe.Tests.Api;

public sealed class ExceptionProblemsTests
{
    public static TheoryData<Exception, int> KnownExceptions() => new()
    {
        { new AuthenticationFailedException(), StatusCodes.Status401Unauthorized },
        { new MissingUserIdentityException(), StatusCodes.Status401Unauthorized },
        { NotFoundException.ForUser(Guid.Empty), StatusCodes.Status404NotFound },
        { new EmailAlreadyRegisteredException(), StatusCodes.Status409Conflict },
        { new LlmUnavailableException("LM Studio could not be reached."), StatusCodes.Status503ServiceUnavailable },
    };

    [Theory]
    [MemberData(nameof(KnownExceptions))]
    public void Maps_application_exceptions_to_their_status_and_message(Exception exception, int status)
    {
        var problem = ExceptionProblems.From(exception);

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(status);
        problem.Detail.Should().Be(exception.Message);
    }

    [Fact]
    public void Maps_a_validation_exception_to_400_with_errors_grouped_by_property()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Email", "'Email' is not a valid email address."),
            new ValidationFailure("Password", "'Password' must not be empty."),
            new ValidationFailure("Password", "'Password' is too short."),
        ]);

        var problem = ExceptionProblems.From(exception);

        var validation = problem.Should().BeOfType<HttpValidationProblemDetails>().Subject;
        validation.Status.Should().Be(StatusCodes.Status400BadRequest);
        validation.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["Email"] = ["'Email' is not a valid email address."],
            ["Password"] = ["'Password' must not be empty.", "'Password' is too short."],
        });
    }

    [Fact]
    public void Leaves_unknown_exceptions_to_the_default_handler()
    {
        ExceptionProblems.From(new InvalidOperationException("boom")).Should().BeNull();
    }
}
