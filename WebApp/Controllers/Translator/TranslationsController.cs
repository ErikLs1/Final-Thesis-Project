using App.Domain.Enum;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
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
    
    [HttpGet]
    public async Task<IActionResult> Index(Guid? languageId,
        int? version,
        TranslationState? state,
        int page = 1,
        int pageSize = 10)
    {
        if (!User.GetUserId(out var userId))
            return Forbid();
        
        var userLanguages = await _bll.UserLanguageService.GetUserKnownLanguagesAsync(userId);
        languageId ??= await _bll.LanguageService.GetDefaultLanguageIdAsync();

        var paging = new PagedRequest { Page = page, PageSize = pageSize };

        var request = new FilteredTranslationsRequestDto(
            languageId.Value,
            version,
            state
        );

        var paged = await _bll
            .UITranslationService
            .GetFilteredUITranslationsAsync(request, paging);

        var vm = new TranslationsIndexVm
        {
            SelectedLanguageId = languageId,
            SelectedVersion = version,
            SelectedState = state,

            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            
            LanguageOptions = userLanguages.Select(l => new LanguageOptionVm
            {
                Id = l.Id,
                Display = l.DisplayValue
            }).ToList(),

            Rows = paged.Items.Select(d => new TranslatorTranslationsRowVm
            {
                ResourceKeyId = d.ResourceKeyId,
                ResourceKey = d.ResourceKey,
                FriendlyKey = d.FriendlyKey,
                Content = d.Content,
                VersionNumber = d.VersionNumber,
                LanguageTag = d.LanguageTag,
                TranslationState = d.TranslationState,
            }).ToList()
        };

        return View(vm);
    }
    
    // PAGE - CREATE NEW TRANSLATION VERSION
    [HttpGet]
    public async Task<IActionResult> CreateVersions(
        Guid? languageId,
        string? q,
        int page = 1,
        int pageSize = 25)
    {
        if (!User.GetUserId(out var userId))
            return Forbid();

        var userLanguages = await _bll.UserLanguageService.GetUserKnownLanguagesAsync(userId);
        var defaultLanguageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();
        var userLanguageIds = userLanguages.Select(l => l.Id).ToHashSet();

        if (!languageId.HasValue || !userLanguageIds.Contains(languageId.Value))
        {
            languageId = userLanguages.FirstOrDefault()?.Id ?? defaultLanguageId;
        }

        var paging = new PagedRequest { Page = page, PageSize = pageSize };

        var paged = await _bll.UITranslationsVersionsService
            .GetFilteredTranslationsAsync(languageId.Value, version: null, paging, q);

        var vm = new CreateVersionsVm
        {
            LanguageId = languageId.Value,

            Search = q,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,

            LanguageOptions = userLanguages.Select(l => new LanguageOptionVm
            {
                Id = l.Id,
                Display = l.DisplayValue
            }).ToList(),

            Items = paged.Items.Select(r => new TranslatorCreateNewVersionItemVm
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
        var userLanguageIds = userLanguages.Select(l => l.Id).ToHashSet();

        if (!userLanguageIds.Contains(vm.LanguageId))
            return Forbid();
        
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
