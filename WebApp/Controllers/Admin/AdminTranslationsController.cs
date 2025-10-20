using App.EF;
using App.Service.BllUow;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers.Admin;

public class AdminTranslationsController : Controller
{
    private readonly IAppBll _bll;

    public AdminTranslationsController(IAppBll bll, AppDbContext db)
    {
        _bll = bll;
    }
    
    [HttpGet]
    public async Task<IActionResult> Index(Guid? languageId)
    {
        var rows = await _bll.UITranslationService.GetLiveTranslationsAsync(languageId);

        var vm = new AdminTranslationsIndexVm
        {
            SelectedLanguageId = languageId,
            Rows = rows.Select(r => new AdminTranslationsIndexVm.Row
            {
                TranslationVersionId = r.TranslationVersionId,
                LanguageTag = r.LanguageTag,
                ResourceKey = r.ResourceKey,
                FriendlyKey = r.FriendlyKey,
                VersionNumber = r.VersionNumber,
                Content = r.Content,
                TranslationState = r.TranslationState,
                PublishedAt = r.PublishedAt,
                PublishedBy = r.PublishedBy
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
            TempData["Error"] = "Update failed. The version may no longer exist.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = $"State updated to {vm.NewState}.";
        return RedirectToAction(nameof(Index));
    }
}