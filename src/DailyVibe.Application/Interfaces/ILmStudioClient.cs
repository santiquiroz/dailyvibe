namespace DailyVibe.Application.Interfaces;

public interface ILmStudioClient
{
    Task<string> GenerateMessageAsync(string intent, CancellationToken cancellationToken = default);
}
