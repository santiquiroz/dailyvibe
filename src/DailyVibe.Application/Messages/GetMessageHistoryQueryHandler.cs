using DailyVibe.Application.Common;
using DailyVibe.Application.Interfaces;
using MediatR;

namespace DailyVibe.Application.Messages;

public sealed class GetMessageHistoryQueryHandler(IDailyMessageRepository messages)
    : IRequestHandler<GetMessageHistoryQuery, PagedResult<DailyMessageDto>>
{
    public async Task<PagedResult<DailyMessageDto>> Handle(GetMessageHistoryQuery request, CancellationToken cancellationToken)
    {
        var page = await messages.GetPageAsync(request.UserId, request.Page, request.Size, cancellationToken);
        return page.Map(DailyMessageDto.From);
    }
}
