using App.EF;
using App.Repository.DTO;
using App.Service.BllUow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers.Translator;

[Authorize]
public class TranslationsController : Controller
{
    private readonly IAppBll _bll;
    private readonly AppDbContext _db;

    public TranslationsController(IAppBll bll, AppDbContext db)
    {
        _bll = bll;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? languageId, int? version)
    {
        var languages = await _db.Languages
            .AsNoTracking()
            .OrderBy(l => l.LanguageName)
            .Select(l => new TranslationsIndexVm.LanguageOption
            {
                Id = l.Id,
                Display = $"{l.LanguageName} ({l.LanguageTag})"
            })
            .ToListAsync();
        
        if (languageId == null)
        {
            languageId = await _db.Languages
                .Where(l => l.IsDefaultLanguage)
                .Select(l => l.Id)
                .SingleAsync();
        }

        // Fetch rows via your repository (latest per key when version == null)
        var rowsDto = await _bll.UITranslationsVersionsService.GetFilteredTranslationsAsync(languageId, version);

        var vm = new TranslationsIndexVm
        {
            SelectedLanguageId = languageId,
            SelectedVersion = version,
            Languages = languages,
            Rows = rowsDto.Select(d => new TranslationsIndexVm.Row
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
    
        [HttpGet]
    public async Task<IActionResult> CreateVersions(Guid? languageId)
    {
        var languages = await _db.Languages
            .OrderBy(l => l.LanguageName)
            .Select(l => new CreateVersionsVm.LanguageOption
            {
                Id = l.Id,
                Display = $"{l.LanguageName} ({l.LanguageTag})"
            })
            .ToListAsync();

        if (languageId is null)
        {
            languageId = await _db.Languages
                .Where(l => l.IsDefaultLanguage)
                .Select(l => l.Id)
                .SingleAsync();
        }

        var defaultLangId = await _db.Languages
            .Where(l => l.IsDefaultLanguage)
            .Select(l => l.Id)
            .SingleAsync();

        var rows = await _bll.UITranslationsVersionsService.GetDefaultLanguageTranslationsAsync();

        var vm = new CreateVersionsVm
        {
            LanguageId = languageId.Value,
            Languages = languages,
            Items = rows.Select(r => new CreateVersionsVm.Item
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

    [HttpPost]
    public async Task<IActionResult> CreateVersions(CreateVersionsVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Languages = await _db.Languages
                .OrderBy(l => l.LanguageName)
                .Select(l => new CreateVersionsVm.LanguageOption
                {
                    Id = l.Id,
                    Display = $"{l.LanguageName} ({l.LanguageTag})"
                })
                .ToListAsync();

            return View(vm);
        }

        var chosen = vm.Items.Where(i => i.Include).ToList();
        if (chosen.Count == 0)
        {
            TempData["Error"] = "Select at least one resource key.";
            return RedirectToAction(nameof(CreateVersions), new { languageId = vm.LanguageId });
        }

        var keyIds = chosen.Select(i => i.ResourceKeyId).ToList();
        var contentMap = chosen.ToDictionary(i => i.ResourceKeyId, i => i.Content ?? string.Empty);

        var createdBy = User?.Identity?.Name ?? "system";

        var req = new CreateVersionRequestDto(
            vm.LanguageId,
            keyIds,
            contentMap,
            createdBy
        );

        await _bll.UITranslationsVersionsService.CreateTranslationVersionsAsync(req);

        TempData["Success"] = $"Created {keyIds.Count} new version(s).";
        return RedirectToAction(nameof(Index), new { languageId = vm.LanguageId });
    }
}