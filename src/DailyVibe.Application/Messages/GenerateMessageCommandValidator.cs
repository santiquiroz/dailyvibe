using FluentValidation;

namespace DailyVibe.Application.Messages;

public sealed class GenerateMessageCommandValidator : AbstractValidator<GenerateMessageCommand>
{
    public GenerateMessageCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.Intent).MaximumLength(MessageRules.IntentMaxLength);
    }
}
