using App.Repository.DTO;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;
using WebApp.Models.Shared;
using WebApp.Models.Translator.Translations;
using WebApp.Models.Translator.Versions;

namespace WebApp.Controllers.Translator;

[Authorize]
public class TranslationsController : Controller
{
    private readonly IAppBll _bll;

    public TranslationsController(IAppBll bll)
    {
        _bll = bll;
    }

    // PAGE - ALL TRANSLATION VERSIONS
    [HttpGet]
    public async Task<IActionResult> Index(Guid? languageId, int? version)
    {
        if (!User.GetUserId(out var userId))
            return Forbid();
        
        // Get user known languages
        var userLanguages = await _bll
            .UserLanguageService
            .GetUserKnownLanguagesAsync(userId);

        // Fallback if language not provided
        if (languageId == null)
        {
            languageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();
        }
        
        // Get translations
        var translations = await _bll
            .UITranslationsVersionsService
            .GetFilteredTranslationsAsync(languageId, version);
        
        // Build vm
        var vm = new TranslationsIndexVm
        {
            SelectedLanguageId = languageId,
            SelectedVersion = version,
            LanguageOptions = userLanguages
                .Select(l => new LanguageOptionVm
                {
                    Id = l.Id,
                    Display = l.DisplayValue
                })
                .ToList(),
            Rows = translations.Select(d => new TranslatorTranslationsRowVm
            {
                ResourceKeyId = d.ResourceKeyId,
                ResourceKey = d.ResourceKey,
                FriendlyKey = d.FriendlyKey,
                Content = d.Content,
                VersionNumber = d.VersionNumber,
                LanguageTag = d.LanguageTag
            }).ToList()
        };

        return View(vm);
    }
    
    // PAGE - CREATE NEW TRANSLATION VERSION
    [HttpGet]
    public async Task<IActionResult> CreateVersions(Guid? languageId)
    {
        if (!User.GetUserId(out var userId))
            return Forbid();
        
        // Get user known languages
        var userLanguages = await _bll
            .UserLanguageService
            .GetUserKnownLanguagesAsync(userId);
        
        // Fallback if language not provided
        if (languageId == null)
        {
            languageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();
        }
    
        var translationVersions = await _bll.UITranslationsVersionsService.GetDefaultLanguageTranslationsAsync();
    
        var vm = new CreateVersionsVm
        {
            LanguageId = languageId.Value,
            LanguageOptions = userLanguages.Select(l => new LanguageOptionVm
            {
                Id = l.Id,
                Display = l.DisplayValue
            }).ToList(),
            Items = translationVersions.Select(r => new TranslatorCreateNewVersionItemVm
            {
                ResourceKeyId = r.ResourceKeyId,
                ResourceKey = r.ResourceKey,
                FriendlyKey = r.FriendlyKey,
                DefaultContent = r.Content,
                Include = false,
                Content = null  
            }).ToList()
        };
    
        return View(vm);
    }
    
    // PAGE - CREATE NEW TRANSLATION VERSION
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVersions(CreateVersionsVm vm)
    {
        
        if (!User.GetUserId(out var userId))
            return Forbid();
        
        // Get user known languages
        var userLanguages = await _bll
            .UserLanguageService
            .GetUserKnownLanguagesAsync(userId);
        
        if (!ModelState.IsValid)
        {
            vm.LanguageOptions = userLanguages
                .Select(l => new LanguageOptionVm
                {
                    Id = l.Id,
                    Display = l.DisplayValue
                })
                .ToList();

            return View(vm);
        }
    
        var chosenTranslations = vm.Items
            .Where(i => i.Include)
            .ToList();
        
        if (chosenTranslations.Count == 0)
        {
            return RedirectToAction(nameof(CreateVersions), new { languageId = vm.LanguageId });
        }
    
        var keyIds = chosenTranslations
            .Select(i => i.ResourceKeyId)
            .ToList();
        
        var contentMap = chosenTranslations.
            ToDictionary(
                i => i.ResourceKeyId, 
                i => i.Content ?? string.Empty
            );
    
        var createdBy = User?.Identity?.Name ?? "system";
    
        var req = new CreateVersionRequestDto(
            vm.LanguageId,
            keyIds,
            contentMap,
            createdBy
        );
    
        await _bll
            .UITranslationsVersionsService
            .CreateTranslationVersionsAsync(req);
    
        return RedirectToAction(nameof(Index), new { languageId = vm.LanguageId });
    }
}