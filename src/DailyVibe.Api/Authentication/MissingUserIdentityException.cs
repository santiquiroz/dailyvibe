namespace DailyVibe.Api.Authentication;

public sealed class MissingUserIdentityException() : Exception("The access token does not identify a user.");
