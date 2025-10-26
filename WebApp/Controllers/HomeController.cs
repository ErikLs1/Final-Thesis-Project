using System.Diagnostics;
using System.Globalization;
using App.Service.BllUow;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.DynamicTranslations;

namespace WebApp.Controllers;

// TODO: REFACTOR
public class HomeController : Controller
{
    private readonly IAppBll _bll;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IAppBll bll)
    {
        _logger = logger;
        _bll = bll;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var tag = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name
                  ?? CultureInfo.CurrentUICulture.Name
                  ?? "en";

        var map = await _bll.UITranslationService.GetLiveTranslationsByLanguageTagAsync(tag);

        var vm = new IndexVm { LanguageTag = tag, Translation = map };
        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}