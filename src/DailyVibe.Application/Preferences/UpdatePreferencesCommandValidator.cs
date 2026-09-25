using DailyVibe.Application.Messages;
using FluentValidation;

namespace DailyVibe.Application.Preferences;

public sealed class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.DefaultIntent).NotEmpty().MaximumLength(MessageRules.IntentMaxLength);
    }
}
