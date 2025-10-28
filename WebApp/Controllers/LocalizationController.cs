using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApp.Controllers;

public class LocalizationController : Controller
{
    private readonly RequestLocalizationOptions _options;

    public LocalizationController(IOptions<RequestLocalizationOptions> options)
    {
        _options = options.Value;
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        var allowed = _options.SupportedUICultures?.Any(c =>
            string.Equals(c.Name, culture, StringComparison.OrdinalIgnoreCase)) == true;

        var target = allowed ? culture : _options.DefaultRequestCulture.UICulture.Name;

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(target)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

        return LocalRedirect(returnUrl);
    }
}