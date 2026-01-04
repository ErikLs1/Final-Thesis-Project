using System.Globalization;
using App.EF;
using App.Repository.DalUow;
using App.Service.Impl.Assemblies.Resx;
using App.Service.Impl.Assemblies.Scanner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Service.Impl.Assemblies.Importer;

public class ResourcesImporter
{
    private readonly AppDbContext _db;
    private readonly IAppUow _uow;
    private readonly ILogger<ResourcesImporter> _logger;
    public ResourcesImporter(
        AppDbContext db,
        IAppUow appUow,
        ILogger<ResourcesImporter> logger)
    {
        _db = db;
        _uow = appUow;
        _logger = logger;
    }

    public async Task ImportFromAssembliesAsync()
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
        
        // DEFAULT LANGUAGE IMPORT
        var neutral = ResourcesScanner.AggregateEntries(CultureInfo.InvariantCulture, _logger, tryParents: false);
        await _uow.ResxImportRepository.ImportFirstTranslationVersionForLanguageAsync(defaultLangId, neutral);

        // REMAINING CULTURES IMPORT
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

            await _uow.ResxImportRepository.ImportFirstTranslationVersionForLanguageAsync(lang.Id, dict);
        }
        
        var keyCount = await _db.UIResourceKeys.CountAsync();
        var verCount = await _db.UITranslationVersions.CountAsync();
        var liveCount = await _db.UITranslations.CountAsync();

        _logger.LogInformation("DB after import: Keys={Keys}, Versions={Versions}, Live={Live}",
            keyCount, verCount, liveCount);

    }
}