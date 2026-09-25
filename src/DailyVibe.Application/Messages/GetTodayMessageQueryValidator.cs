using FluentValidation;

namespace DailyVibe.Application.Messages;

public sealed class GetTodayMessageQueryValidator : AbstractValidator<GetTodayMessageQuery>
{
    public GetTodayMessageQueryValidator()
    {
        RuleFor(q => q.UserId).NotEmpty();
    }
}
