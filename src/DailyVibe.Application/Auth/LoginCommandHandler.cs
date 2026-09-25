using System.Diagnostics.CodeAnalysis;
using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using MediatR;

namespace DailyVibe.Application.Auth;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(CredentialRules.NormalizeEmail(request.Email), cancellationToken);
        if (!HasPassword(user, request.Password))
        {
            throw new AuthenticationFailedException();
        }

        return new AuthResult(user.Id, user.Email, jwtService.GenerateToken(user));
    }

    private bool HasPassword([NotNullWhen(true)] User? user, string password) =>
        user is not null && passwordHasher.Verify(password, user.PasswordHash);
}
