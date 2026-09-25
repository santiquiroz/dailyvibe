using DailyVibe.Application.Common;
using MediatR;

namespace DailyVibe.Application.Messages;

public sealed record GetMessageHistoryQuery(Guid UserId, int Page = 1, int Size = MessageRules.DefaultPageSize)
    : IRequest<PagedResult<DailyMessageDto>>;
