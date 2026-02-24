using App.Domain.Enum;
using App.Domain.Identity;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApp.Helpers;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Models.Admin.Audit;
using WebApp.Models.Admin.Translations;
using WebApp.Models.Shared;

namespace WebApp.Controllers.Admin;

[Authorize(Roles = nameof(RoleType.Admin))] 
[AutoValidateAntiforgeryToken] 
public class AdminController : Controller
{
    private readonly IAppBll _bll;
    private readonly ITranslationCache _cache;
    private readonly UserManager<AppUser> _userManager;

    public AdminController(
        IAppBll bll,
        ITranslationCache cache,
        UserManager<AppUser> userManager)
    {
        _bll = bll;
        _cache = cache;
        _userManager = userManager;
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
         
         var effectiveState = state ?? TranslationState.Published;
         
         var request = new FilteredTranslationsRequestDto(
             selectedLangId,
             version,
             State: effectiveState,
             States: null
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
             SelectedState = effectiveState,

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

         // Show translations that can be published by admin.
         var request = new FilteredTranslationsRequestDto(
             selectedLangId,
             VersionNumber: null,
             State: null,
             States: new[] { TranslationState.Approved, TranslationState.Inactive }
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
    [EnableRateLimiting("AdminPublishPolicy")]
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

    [HttpGet]
    public async Task<IActionResult> Audit(
        Guid? languageId,
        TranslationAuditAction? actionType,
        string? changedBy,
        string? resourceKeySearch,
        DateTime? changedFromUtc,
        DateTime? changedToUtc,
        int page = 1,
        int pageSize = 25)
    {
        var allLanguages = await _bll.LanguageService.GetAllLanguages();

        var paging = new PagedRequest
        {
            Page = page,
            PageSize = pageSize
        };

        var request = new FilteredTranslationAuditRequestDto(
            languageId,
            actionType,
            changedBy,
            resourceKeySearch,
            changedFromUtc,
            changedToUtc);

        var paged = await _bll.UITranslationAuditLogService.GetAuditLogsAsync(request, paging);

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
        
        var changedByOptions = await GetAuditActorOptionsAsync();

        var vm = new AdminTranslationAuditIndexVm
        {
            SelectedLanguageId = languageId,
            SelectedActionType = actionType,
            ChangedBy = changedBy,
            ResourceKeySearch = resourceKeySearch,
            ChangedFromUtc = changedFromUtc,
            ChangedToUtc = changedToUtc,
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount,
            LanguageOptions = languageOptions,
            ChangedByOptions = changedByOptions,
            Rows = paged.Items.Select(a => new AdminTranslationAuditIndexRowVm
            {
                ChangedAt = a.ChangedAt,
                ChangedBy = a.ChangedBy,
                ActionType = a.ActionType,
                LanguageTag = a.LanguageTag,
                ResourceKey = a.ResourceKey,
                FriendlyKey = a.FriendlyKey,
                VersionNumber = a.VersionNumber,
                OldState = a.OldState,
                NewState = a.NewState,
                OldContent = a.OldContent,
                NewContent = a.NewContent
            }).ToList()
        };

        return View(vm);
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

    private async Task<List<string>> GetAuditActorOptionsAsync()
    {
        var allowedRoles = new[] { nameof(RoleType.Admin), nameof(RoleType.Reviewer), nameof(RoleType.Translator) };
        var usernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var role in allowedRoles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.UserName))
                {
                    usernames.Add(user.UserName);
                }
            }
        }

        return usernames
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
