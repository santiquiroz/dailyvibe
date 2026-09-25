using Microsoft.AspNetCore.Diagnostics;

namespace DailyVibe.Api.ErrorHandling;

public sealed class ProblemDetailsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = ExceptionProblems.From(exception);
        if (problem?.Status is not { } status)
        {
            return false;
        }

        httpContext.Response.StatusCode = status;
        // The status code already answers the client even when its Accept header rules out a problem body.
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
        return true;
    }
}
