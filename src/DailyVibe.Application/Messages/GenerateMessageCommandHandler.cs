using DailyVibe.Application.Exceptions;
using DailyVibe.Application.Interfaces;
using DailyVibe.Domain.Entities;
using MediatR;

namespace DailyVibe.Application.Messages;

public sealed class GenerateMessageCommandHandler(
    IUserRepository users,
    IDailyMessageRepository messages,
    ILmStudioClient lmStudioClient,
    TimeProvider timeProvider) : IRequestHandler<GenerateMessageCommand, DailyMessageDto>
{
    public async Task<DailyMessageDto> Handle(GenerateMessageCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw NotFoundException.ForUser(request.UserId);
        var intent = MessageRules.ResolveIntent(request.Intent, user.DefaultIntent);
        var content = await lmStudioClient.GenerateMessageAsync(intent, cancellationToken);

        var message = CreateMessage(user.Id, content, intent);
        await messages.AddAsync(message, cancellationToken);

        return DailyMessageDto.From(message);
    }

    private DailyMessage CreateMessage(Guid userId, string content, string intent) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Content = content,
        Intent = intent,
        CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
    };
}
