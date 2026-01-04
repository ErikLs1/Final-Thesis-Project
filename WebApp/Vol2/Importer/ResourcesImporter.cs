using System.Globalization;
using App.EF;
using App.Repository.Impl.ResxImport;
using Microsoft.EntityFrameworkCore;
using WebApp.Vol2.Resx;
using WebApp.Vol2.Scanner;

namespace WebApp.Vol2.Importer;

public class ResourcesImporter
{
    private readonly AppDbContext _db;
    private readonly ResxImportRepository _resxImportRepository;
    private readonly ILogger<ResourcesImporter> _logger;
    public ResourcesImporter(
        AppDbContext db,
        ResxImportRepository resxImportRepository, 
        ILogger<ResourcesImporter> logger)
    {
        _db = db;
        _resxImportRepository = resxImportRepository;
        _logger = logger;
    }

    public async Task ImportFromAssembliesAsync(string publishedBy = "resx-startup-import",
        string createdBy = "resx-import")
    {
        var resourceManagers = ResourceManagerRegistry.All;
        if (resourceManagers.Count == 0)
        {
            _logger.LogWarning("No ResourceManagers found from dependency context. ASSEMBLY IMPORT SKIPPED.");
            return;
        }
        
        var languages = await _db.Languages
            .Select(l => new { l.Id, l.LanguageTag, l.IsDefaultLanguage })
            .ToListAsync();

        var defaultLangId = languages
            .Where(x => x.IsDefaultLanguage)
            .Select(x => x.Id)
            .SingleOrDefault();

        if (defaultLangId == Guid.Empty)
        {
            _logger.LogWarning("No default language found. ASSEMBLY IMPORT SKIPPED");
            return;
        }
        
        // 1) Neutral -> default language
        var neutral = ResourcesScanner.AggregateEntries(CultureInfo.InvariantCulture, _logger, tryParents: false);
        await _resxImportRepository.ImportFirstTranslationVersionForLanguageAsync(defaultLangId, neutral, createdBy);

        // 2) Culture-specific -> DB languages
        foreach (var lang in languages.Where(l => !l.IsDefaultLanguage))
        {
            if (string.IsNullOrWhiteSpace(lang.LanguageTag)) continue;

            CultureInfo culture;
            try { culture = CultureInfo.GetCultureInfo(lang.LanguageTag); }
            catch (CultureNotFoundException)
            {
                _logger.LogWarning("Skipping invalid culture tag in DB: {Tag}", lang.LanguageTag);
                continue;
            }

            var dict = ResourcesScanner.AggregateEntries(culture, _logger, tryParents: false);
            if (dict.Count == 0) continue;

            await _resxImportRepository.ImportFirstTranslationVersionForLanguageAsync(lang.Id, dict, createdBy);
        }

        // Keep this until you fold the "live pointer creation" into the per-language import method
        var inserted = await _resxImportRepository.InialUITranslationsImportAsync(publishedBy);
        _logger.LogInformation("ASSEMBLY IMPORT COMPLETED. UITranslations inserted: {Count}", inserted);
        
        var keyCount = await _db.UIResourceKeys.CountAsync();
        var verCount = await _db.UITranslationVersions.CountAsync();
        var liveCount = await _db.UITranslations.CountAsync();

        _logger.LogInformation("DB after import: Keys={Keys}, Versions={Versions}, Live={Live}",
            keyCount, verCount, liveCount);

    }
}