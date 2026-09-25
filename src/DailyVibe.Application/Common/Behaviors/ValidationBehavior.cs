using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace DailyVibe.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await CollectFailuresAsync(request, cancellationToken);
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }

    private async Task<List<ValidationFailure>> CollectFailuresAsync(TRequest request, CancellationToken cancellationToken)
    {
        // A shared ValidationContext accumulates failures, so each validator gets its own to avoid duplicates.
        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));
        return results.SelectMany(r => r.Errors).ToList();
    }
}
