using DailyVibe.Application.Common;
using DailyVibe.Application.Interfaces;
using MediatR;

namespace DailyVibe.Application.Messages;

public sealed class GetTodayMessageQueryHandler(
    IDailyMessageRepository messages,
    ISender sender,
    TimeProvider timeProvider) : IRequestHandler<GetTodayMessageQuery, DailyMessageDto>
{
    public async Task<DailyMessageDto> Handle(GetTodayMessageQuery request, CancellationToken cancellationToken)
    {
        var (start, end) = UtcDay.Containing(timeProvider.GetUtcNow());
        var existing = await messages.FindLatestCreatedBetweenAsync(request.UserId, start, end, cancellationToken);
        if (existing is not null)
        {
            return DailyMessageDto.From(existing);
        }

        return await sender.Send(new GenerateMessageCommand(request.UserId), cancellationToken);
    }
}
