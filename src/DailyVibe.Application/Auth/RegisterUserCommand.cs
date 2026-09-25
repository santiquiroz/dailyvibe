using MediatR;

namespace DailyVibe.Application.Auth;

public sealed record RegisterUserCommand(string Email, string Password) : IRequest<AuthResult>;
