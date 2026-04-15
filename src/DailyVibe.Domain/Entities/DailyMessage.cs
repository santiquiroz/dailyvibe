namespace DailyVibe.Domain.Entities;

public class DailyMessage
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = default!;
    public string Intent { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}
