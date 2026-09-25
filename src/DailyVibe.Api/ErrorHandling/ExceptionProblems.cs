using DailyVibe.Api.Authentication;
using DailyVibe.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace DailyVibe.Api.ErrorHandling;

public static class ExceptionProblems
{
    public static ProblemDetails? From(Exception exception) => exception switch
    {
        ValidationException validation => ForValidation(validation.Errors),
        AuthenticationFailedException or MissingUserIdentityException => ForStatus(StatusCodes.Status401Unauthorized, exception),
        NotFoundException => ForStatus(StatusCodes.Status404NotFound, exception),
        EmailAlreadyRegisteredException => ForStatus(StatusCodes.Status409Conflict, exception),
        LlmUnavailableException => ForStatus(StatusCodes.Status503ServiceUnavailable, exception),
        _ => null,
    };

    private static ProblemDetails ForStatus(int status, Exception exception) =>
        new() { Status = status, Detail = exception.Message };

    private static HttpValidationProblemDetails ForValidation(IEnumerable<ValidationFailure> failures) =>
        new(GroupByProperty(failures))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
        };

    private static Dictionary<string, string[]> GroupByProperty(IEnumerable<ValidationFailure> failures) =>
        failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
}
