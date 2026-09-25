namespace DailyVibe.Application.Auth;

public sealed record AuthResult(Guid UserId, string Email, string Token);
