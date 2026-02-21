using App.Domain.Enum;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Helpers;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Models.Admin.Translations;
using WebApp.Models.Shared;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = "Admin")] 
[AutoValidateAntiforgeryToken] 
public class AdminTranslationsController : Controller
{
    private readonly IAppBll _bll;
    private readonly ITranslationCache _cache;

    public AdminTranslationsController(
        IAppBll bll,
        ITranslationCache cache)
    {
        _bll = bll;
        _cache = cache;
    }

     [HttpGet]
     public async Task<IActionResult> Index(
         Guid? languageId, 
         int? version,
         TranslationState? state,
         int page = 1,
         int pageSize = 10)
     {
         
         if (!User.GetUserId(out var userId))
             return Forbid();
         
         var (selectedLangId, langTag, allLanguages) = await ResolveLanguageAsync(languageId);
         
         var paging = new PagedRequest
         {
             Page = page,
             PageSize = pageSize
         };
         
         var request = new FilteredTranslationsRequestDto(
             selectedLangId,
             version,
             state
         );

         var paged = await _bll.UITranslationService.GetFilteredUITranslationsAsync(request, paging);

         var languageOptions = allLanguages
             .OrderBy(l => l.Name)
             .Select(l => new LanguageOptionVm
             {
                 Id = l.Id,
                 Display = $"{l.Name} ({l.Tag})",
                 LanguageName = l.Name,
                 LanguageTag = l.Tag
             })
             .ToList();

         var vm = new AdminTranslationsIndexVm
         {
             SelectedLanguageId = selectedLangId,
             SelectedVersion = version,
             SelectedState = state,

             Page = paged.Page,
             PageSize = paged.PageSize,
             TotalCount = paged.TotalCount,

             LanguageOptions = languageOptions,
             Rows = paged.Items.Select(r => new AdminTranslationsIndexRowVm
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
     public async Task<IActionResult> Publish(
         Guid? languageId,
         int page = 1,
         int pageSize = 25)
     {
         var (selectedLangId, langTag, _) = await ResolveLanguageAsync(languageId);

         var paging = new PagedRequest { Page = page, PageSize = pageSize };

         // Show newly submitted translations so admin can publish them.
         var request = new FilteredTranslationsRequestDto(
             selectedLangId,
             VersionNumber: null,
             State: TranslationState.WaitingReview
         );

         var paged = await _bll.UITranslationService.GetFilteredUITranslationsAsync(request, paging);

         var vm = new AdminPublishTranslationsVm
         {
             SelectedLanguageId = selectedLangId,
             SelectedLanguageTag = langTag,

             Page = paged.Page,
             PageSize = paged.PageSize,
             TotalCount = paged.TotalCount,

             Rows = paged.Items.Select(v => new AdminPublishTranslationVersionVm
             {
                 TranslationVersionId = v.TranslationVersionId,
                 FriendlyKey = v.FriendlyKey,
                 Content = v.Content,
                 VersionNumber = v.VersionNumber,
                 LanguageTag = v.LanguageTag,
                 CurrentState = v.TranslationState.ToString(),
                 Selected = false
             }).ToList()
         };

         return View(vm);
     }

    [HttpPost]
    public async Task<IActionResult> Publish(AdminPublishTranslationsVm vm)
    {
        var chosenVersionIds = vm.Rows
            .Where(r => r.Selected)
            .Select(r => r.TranslationVersionId)
            .Distinct()
            .ToList();

        if (chosenVersionIds.Count == 0)
        {
            return RedirectToAction(nameof(Publish), new { languageId = vm.SelectedLanguageId });
        }

        var activatedBy = User?.Identity?.Name ?? "system";

        var publishRequests = chosenVersionIds
            .Select(id => new PublishTranslationVersionRequestDto(
                id,
                activatedBy
            ))
            .ToList();

        await _bll.UITranslationService.PublishTranslationTranslationsAsync(publishRequests);
        
        var langTag = vm.SelectedLanguageTag;

        // invalidate / refresh
        if (!string.IsNullOrWhiteSpace(langTag))
        {
            await _cache.InvalidateAsync(langTag);
            await _cache.GetLanguageMapAsync(langTag);
        }

        return RedirectToAction(nameof(Index), new { languageId = vm.SelectedLanguageId });
    }
    
    private async Task<(Guid selectedLangId, string langTag, IReadOnlyList<LanguageDto> allLanguages)>
        ResolveLanguageAsync(Guid? languageId)
    {
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var defaultLanguageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();

        var selectedLangId = languageId ?? defaultLanguageId;

        var lang = allLanguages.FirstOrDefault(l => l.Id == selectedLangId);
        if (lang == null)
        {
            selectedLangId = defaultLanguageId;
            lang = allLanguages.First(l => l.Id == selectedLangId);
        }

        return (selectedLangId, lang.Tag, allLanguages);
    }
}
