namespace DailyVibe.Application.Exceptions;

public sealed class LlmUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
