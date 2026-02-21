using App.Domain.Enum;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.Reviewer;
using WebApp.Models.Shared;

namespace WebApp.Controllers.Reviewer;

[Authorize(Roles = "Reviewer")]
[AutoValidateAntiforgeryToken]
public class ReviewerController : Controller
{
    private readonly IAppBll _bll;

    public ReviewerController(IAppBll bll)
    {
        _bll = bll;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? languageId,
        int? version,
        int page = 1,
        int pageSize = 10)
    {
        var allLanguages = await _bll.LanguageService.GetAllLanguages();
        var defaultLanguageId = await _bll.LanguageService.GetDefaultLanguageIdAsync();

        var selectedLanguageId = languageId ?? defaultLanguageId;
        if (allLanguages.All(l => l.Id != selectedLanguageId))
        {
            selectedLanguageId = defaultLanguageId;
        }

        var paging = new PagedRequest { Page = page, PageSize = pageSize };
        var request = new FilteredTranslationsRequestDto(
            selectedLanguageId,
            version,
            TranslationState.WaitingReview
        );

        var paged = await _bll.UITranslationService.GetFilteredUITranslationsAsync(request, paging);

        var vm = new ReviewerQueueVm
        {
            SelectedLanguageId = selectedLanguageId,
            SelectedVersion = version,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            LanguageOptions = allLanguages
                .OrderBy(l => l.Name)
                .Select(l => new LanguageOptionVm
                {
                    Id = l.Id,
                    Display = $"{l.Name} ({l.Tag})",
                    LanguageName = l.Name,
                    LanguageTag = l.Tag
                })
                .ToList(),
            Rows = paged.Items.Select(r => new ReviewerQueueRowVm
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

    [HttpPost]
    public async Task<IActionResult> Decide(
        Guid translationVersionId,
        Guid selectedLanguageId,
        int? selectedVersion,
        int page = 1,
        int pageSize = 10,
        TranslationState decision = TranslationState.Approved)
    {
        if (decision != TranslationState.Approved && decision != TranslationState.Rejected)
        {
            return BadRequest("Invalid decision.");
        }

        var updatedBy = User?.Identity?.Name ?? "system";

        await _bll.UITranslationService.UpdateTranslationStateAsync(
            new UpdateTranslationStateRequestDto(
                translationVersionId,
                decision,
                updatedBy
            ));

        return RedirectToAction(nameof(Index), new
        {
            languageId = selectedLanguageId,
            version = selectedVersion,
            page,
            pageSize
        });
    }
}
