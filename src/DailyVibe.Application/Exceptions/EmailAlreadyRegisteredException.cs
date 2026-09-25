namespace DailyVibe.Application.Exceptions;

public sealed class EmailAlreadyRegisteredException(Exception? innerException = null)
    : Exception("The email is already registered.", innerException);
