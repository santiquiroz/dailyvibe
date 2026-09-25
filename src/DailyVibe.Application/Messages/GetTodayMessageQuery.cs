using MediatR;

namespace DailyVibe.Application.Messages;

public sealed record GetTodayMessageQuery(Guid UserId) : IRequest<DailyMessageDto>;
