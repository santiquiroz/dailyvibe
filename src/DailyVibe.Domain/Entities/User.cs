namespace DailyVibe.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string DefaultIntent { get; set; } = "motivacional y estoico, en español, 2 frases máx";
    public DateTime CreatedAt { get; set; }

    public ICollection<DailyMessage> DailyMessages { get; set; } = [];
}
