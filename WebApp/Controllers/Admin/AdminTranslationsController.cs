using App.Domain.Enum;
using App.EF;
using App.Repository.DTO.UITranslations;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.Admin.Translations;
using WebApp.Models.Shared;
using WebApp.Redis.Services;

namespace WebApp.Controllers.Admin;

public class AdminTranslationsController : Controller
{
    private readonly IAppBll _bll;
    private readonly IRedisTranslationService _redisTranslationService;

    public AdminTranslationsController(
        IAppBll bll,
        IRedisTranslationService redisTranslationService)
    {
        _bll = bll;
        _redisTranslationService = redisTranslationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
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
        if (languageId == null)
        {
            languageId = defaultLanguage;
        }

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
    }

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
            await _redisTranslationService.RefreshTranslationAsync(langTag);
        }

        return RedirectToAction(nameof(Index), new { languageId = vm.SelectedLanguageId });
    }
}