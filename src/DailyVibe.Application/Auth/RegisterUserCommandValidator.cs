using FluentValidation;

namespace DailyVibe.Application.Auth;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(CredentialRules.EmailMaxLength);

        RuleFor(c => c.Password)
            .NotEmpty()
            .MinimumLength(CredentialRules.PasswordMinLength)
            .Must(CredentialRules.FitsBcryptLimit)
            .WithMessage($"'Password' must not exceed {CredentialRules.PasswordMaxBytes} bytes.");
    }
}
