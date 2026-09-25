using MediatR;

namespace DailyVibe.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;
