using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DailyVibe.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(FindSubject(principal), out var userId) ? userId : throw new MissingUserIdentityException();

    // JwtBearer maps "sub" to NameIdentifier unless inbound claim mapping is turned off.
    private static string? FindSubject(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
