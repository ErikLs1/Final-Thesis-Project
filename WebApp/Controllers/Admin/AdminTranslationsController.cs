using App.Domain.Enum;
using App.EF;
using App.Repository.DTO.UITranslations;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Extensions.Pager.models;
using WebApp.Helpers;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Models.Admin.Translations;
using WebApp.Models.Shared;
using WebApp.Redis.Services;

namespace WebApp.Controllers.Admin;

public class AdminTranslationsController : Controller
{
    private readonly IAppBll _bll;
    private readonly IRedisTranslationService _redisTranslationService;
    private readonly ITranslationCache _cache;

    public AdminTranslationsController(
        IAppBll bll,
        IRedisTranslationService redisTranslationService,
        ITranslationCache cache)
    {
        _bll = bll;
        _redisTranslationService = redisTranslationService;
        _cache = cache;
    }

    /*[HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(
        Guid? languageId, 
        int? version,
        TranslationState? state,
        int page = 1,
        int pageSize = 10)
    {
        
        if (!User.GetUserId(out var userId))
            return Forbid();

        // Get user known languages
        var userLanguages = await _bll
            .UserLanguageService
            .GetUserKnownLanguagesAsync(userId);
        
        // Fallback to default lang
        languageId ??= await _bll.LanguageService.GetDefaultLanguageIdAsync();

        var paging = new PagedRequest
        {
            Page = page,
            PageSize = pageSize
        };

        var paged = await _bll
            .UITranslationsVersionsService
            .GetFilteredTranslationsAsync(languageId.Value, version, state, paging);

        var request = new FilteredTranslationsRequestDto(
            languageId.Value,
            version,
            state
        );

        var filteredTranslations = await _bll
            .UITranslationService
            .GetFilteredUITranslationsAsync(request);

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
            Rows = filteredTranslations.Select(r => new AdminTranslationsIndexRowVm
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Publish(Guid? languageId)
    {
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var defaultLanguageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();

        if (languageId == null)
        {
            languageId = defaultLanguageId;
        }

        var langInfo = allLanguages.FirstOrDefault(l => l.Id == languageId.Value)
                       ?? allLanguages.First();
        var langId = langInfo.Id;
        var langTag = langInfo.Tag;


        var filterReq = new FilteredTranslationsRequestDto(
            langId,
            VersionNumber: null,
            State: null
        );

        var allVersions = await _bll
            .UITranslationService
            .GetFilteredUITranslationsAsync(filterReq);

        var vm = new AdminPublishTranslationsVm
        {
            SelectedLanguageId = langId,
            SelectedLanguageTag = langTag,
            Rows = allVersions
                .Select(v => new AdminPublishTranslationVersionVm
                {
                    TranslationVersionId = v.TranslationVersionId,
                    FriendlyKey = v.FriendlyKey,
                    Content = v.Content,
                    VersionNumber = v.VersionNumber,
                    LanguageTag = v.LanguageTag,
                    CurrentState = v.TranslationState.ToString(),
                    Selected = false
                })
                .ToList()
        };

        return View(vm);
    }*/

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
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

        // REFRESH REDIS
        var langTag = vm.SelectedLanguageTag;

        if (!string.IsNullOrWhiteSpace(langTag))
        {
            await _cache.InvalidateAsync(langTag);
            await _cache.GetLanguageMapAsync(langTag);
        }

        return RedirectToAction(nameof(Index), new { languageId = vm.SelectedLanguageId });
    }
}