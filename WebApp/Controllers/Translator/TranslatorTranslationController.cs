using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Controllers.Translator;

[Authorize]
public class TranslatorTranslationController : Controller
{
    private readonly IAppBll _bll;

    public TranslatorTranslationController(IAppBll bll)
    {
        _bll = bll;
    }

    // OVERVIEW
    [HttpGet]
    public async Task<IActionResult> MyLanguages()
    {
        var userId = User.GetUserId();
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var userLanguages = await _bll.UserLanguageService.GetUserLanguageIdsAsync(userId);
        var selected = allLanguages
            .Where(l => userLanguages.Contains(l.Id))
            .OrderBy(l => l.Name)
            .Select(l => new LanguageRow { Id = l.Id, Tag = l.Tag, Name = l.Name })
            .ToList();

        var vm = new MyLanguagesVm { Selected = selected };
        return View(vm);
    }
    
    // SELECTION PAGE
    [HttpGet]
    public async Task<IActionResult> Languages()
    {
        var userId = User.GetUserId();
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var userLanguages = await _bll.UserLanguageService.GetUserLanguageIdsAsync(userId);

        var vm = new TranslatorLanguagesVm
        {
            Languages = allLanguages.Select(x => new LanguageVmDto
            {
                Id = x.Id,
                Display = $"{x.Name} ({x.Tag})",
                Selected = userLanguages.Contains(x.Id)
            }).ToList()
        };

        return View("LanguagesSelection", vm);
    }
    
    // SELECTION PAGE
    [HttpPost]
    public async Task<IActionResult> Languages(TranslatorLanguagesVm vm)
    {
        var userId = User.GetUserId();
        var selectedLanguages = vm.Languages.Where(x => x.Selected).Select(x => x.Id);
        await _bll.UserLanguageService.UpdateUserLanguagesAsync(userId, selectedLanguages);
        
        TempData["Success"] = "Languages were successfully saved.";
        return RedirectToAction(nameof(MyLanguages));
    }
}