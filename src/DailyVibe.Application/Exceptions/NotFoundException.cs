namespace DailyVibe.Application.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException ForUser(Guid userId) => new($"User '{userId}' was not found.");
}
