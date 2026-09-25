using FluentValidation;

namespace DailyVibe.Application.Messages;

public sealed class GetMessageHistoryQueryValidator : AbstractValidator<GetMessageHistoryQuery>
{
    public GetMessageHistoryQueryValidator()
    {
        RuleFor(q => q.UserId).NotEmpty();
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.Size).InclusiveBetween(1, MessageRules.MaxPageSize);
    }
}
