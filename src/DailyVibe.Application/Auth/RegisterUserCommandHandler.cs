using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using MediatR;

namespace DailyVibe.Application.Auth;

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    TimeProvider timeProvider) : IRequestHandler<RegisterUserCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var email = CredentialRules.NormalizeEmail(request.Email);
        if (await users.EmailExistsAsync(email, cancellationToken))
        {
            throw new EmailAlreadyRegisteredException();
        }

        var user = CreateUser(email, request.Password);
        await users.AddAsync(user, cancellationToken);

        return new AuthResult(user.Id, user.Email, jwtService.GenerateToken(user));
    }

    private User CreateUser(string email, string password) => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        PasswordHash = passwordHasher.Hash(password),
        CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
    };
}
