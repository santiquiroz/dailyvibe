using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using MediatR;

namespace DailyVibe.Application.Preferences;

public sealed class UpdatePreferencesCommandHandler(IUserRepository users) : IRequestHandler<UpdatePreferencesCommand>
{
    public async Task Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var updated = await users.UpdateDefaultIntentAsync(request.UserId, request.DefaultIntent.Trim(), cancellationToken);
        if (!updated)
        {
            throw NotFoundException.ForUser(request.UserId);
        }
    }
}
