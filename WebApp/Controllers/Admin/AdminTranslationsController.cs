using App.Domain.Enum;
using App.EF;
using App.Service.BllUow;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.Admin.Translations;
using WebApp.Models.Shared;

namespace WebApp.Controllers.Admin;

public class AdminTranslationsController : Controller
{
    private readonly IAppBll _bll;

    public AdminTranslationsController(IAppBll bll, AppDbContext db)
    {
        _bll = bll;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index(Guid? languageId, int? version, TranslationState? state)
    {
        // Get all languages
        var allLanguages = await _bll
            .LanguageService
            .GetAllLanguages();
        
        // Get default language
        var defaultLanguage = await _bll
            .LanguageService
            .GetDefaultLanguageIdAsync();
        
        
        var liveTranslations = await _bll
            .UITranslationService
            .GetLiveTranslationsAsync(languageId);

        if (version.HasValue)
        {
            liveTranslations = liveTranslations
                .Where(r => r.VersionNumber == version.Value)
                .ToList();
        }
        
        if (state.HasValue)
        {
            liveTranslations = liveTranslations
                .Where(r => r.TranslationState == state.Value)
                .ToList();
        }
        
        var languageOptions = allLanguages
            .OrderBy(l => l.Name)
            .Select(l => new LanguageOptionVm
            {
                Id = l.Id,
                Display = $"{l.Name} ({l.Tag})",
                LanguageName = l.Name,
                LanguageTag = l.Tag,
            })
            .ToList();
        
        var vm = new AdminTranslationsIndexVm
        {
            SelectedLanguageId = languageId,
            LanguageOptions = languageOptions,
            Rows = liveTranslations.Select(r => new AdminTranslationsIndexRowVm
            {
                TranslationVersionId = r.TranslationVersionId,
                LanguageTag = r.LanguageTag,
                ResourceKey = r.ResourceKey,
                FriendlyKey = r.FriendlyKey,
                VersionNumber = r.VersionNumber,
                Content = r.Content,
                TranslationState = r.TranslationState
            }).ToList()
        };

        return View(vm);
    }
    
    [HttpGet]
    public async Task<IActionResult> ChangeState(Guid versionId)
    {
        var all = await _bll.UITranslationService.GetLiveTranslationsAsync(null);
        var row = all.FirstOrDefault(x => x.TranslationVersionId == versionId);
        if (row == null) return NotFound();

        var vm = new ChangeStateVm
        {
            TranslationVersionId = row.TranslationVersionId,
            LanguageTag = row.LanguageTag,
            ResourceKey = row.ResourceKey,
            FriendlyKey = row.FriendlyKey,
            VersionNumber = row.VersionNumber,
            CurrentState = row.TranslationState,
            Content = row.Content
        };

        return View(vm);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeState(ChangeStateVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var req = new App.Repository.DTO.UpdateTranslationStateRequestDto(
            vm.TranslationVersionId,
            vm.NewState,
            User?.Identity?.Name ?? "system"
        );

        var changed = await _bll.UITranslationService.UpdateTranslationStateAsync(req);
        if (changed == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"State updated to {vm.NewState}.";
        return RedirectToAction(nameof(Index));
    }
}