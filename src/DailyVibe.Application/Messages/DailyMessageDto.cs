using DailyVibe.Domain.Entities;

namespace DailyVibe.Application.Messages;

public sealed record DailyMessageDto(Guid Id, string Content, string Intent, DateTime CreatedAt)
{
    public static DailyMessageDto From(DailyMessage message) =>
        new(message.Id, message.Content, message.Intent, message.CreatedAt);
}
