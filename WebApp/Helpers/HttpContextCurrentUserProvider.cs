using System.Security.Claims;
using App.EF;

namespace WebApp.Helpers;

public sealed class HttpContextCurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return null;

        return user.Identity?.Name
               ?? user.FindFirstValue(ClaimTypes.Email)
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
