using MediatR;

namespace DailyVibe.Application.Messages;

public sealed record GenerateMessageCommand(Guid UserId, string? Intent = null) : IRequest<DailyMessageDto>;
