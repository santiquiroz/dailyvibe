using MediatR;

namespace DailyVibe.Application.Preferences;

public sealed record UpdatePreferencesCommand(Guid UserId, string DefaultIntent) : IRequest;
