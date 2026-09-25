using System.Text;

namespace DailyVibe.Application.Auth;

public static class CredentialRules
{
    public const int EmailMaxLength = 256;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxBytes = 72;

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // BCrypt ignores every byte after the 72nd, so a longer password would verify by its prefix alone.
    public static bool FitsBcryptLimit(string? password) =>
        password is null || Encoding.UTF8.GetByteCount(password) <= PasswordMaxBytes;
}
