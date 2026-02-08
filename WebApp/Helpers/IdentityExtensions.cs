using System.Security.Claims;

namespace WebApp.Helpers;

public static class IdentityExtensions
{
    public static bool GetUserId(this ClaimsPrincipal? claimsPrincipal, out Guid userId)
    {
        userId = default;

        var raw = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out userId);
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal.GetUserId(out var id)) return id;
        throw new UnauthorizedAccessException("Missing claim.");
    }
}