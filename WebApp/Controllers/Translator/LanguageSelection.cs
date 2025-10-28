using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;
using WebApp.Models.Translator.Languages;

namespace WebApp.Controllers.Translator;

[Authorize]
public class LanguageSelection : Controller
{
    private readonly IAppBll _bll;

    public LanguageSelection(IAppBll bll)
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
            .Select(l => new TranslatorKnownLanguageRowVm
            {
                Id = l.Id, 
                Tag = l.Tag, 
                Name = l.Name
            })
            .ToList();

        var vm = new TranslatorMyLanguagesVm
        {
            Selected = selected 
        };
        
        return View(vm);
    }
    
    // SELECTION PAGE
    [HttpGet]
    public async Task<IActionResult> Languages()
    {
        var userId = User.GetUserId();
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var userLanguages = await _bll.UserLanguageService.GetUserLanguageIdsAsync(userId);

        var vm = new TranslatorLanguagesSelectionVm
        {
            Languages = allLanguages.Select(x => new TranslatorLanguageSelectionItemVm
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
    public async Task<IActionResult> Languages(TranslatorLanguagesSelectionVm vm)
    {
        var userId = User.GetUserId();
        var selectedLanguages = vm.Languages
            .Where(x => x.Selected)
            .Select(x => x.Id);
        
        await _bll.UserLanguageService.UpdateUserLanguagesAsync(userId, selectedLanguages);
        
        TempData["Success"] = "Languages were successfully saved.";
        return RedirectToAction(nameof(MyLanguages));
    }
}