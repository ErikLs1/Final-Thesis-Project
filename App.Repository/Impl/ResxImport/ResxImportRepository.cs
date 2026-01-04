using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Repository.Impl.ResxImport;

public class ResxImportRepository : IResxImportRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ResxImportRepository> _logger;
    
    public ResxImportRepository(AppDbContext repositoryDbContext, ILogger<ResxImportRepository> logger)
    {
        _db = repositoryDbContext;
        _logger = logger;
    }
    
    

   // https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-resources-resourcereader
    // https://learn.microsoft.com/en-us/dotnet/api/system.resources.resxresourcereader?view=windowsdesktop-9.0
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-9.0

    public async Task ImportFirstTranslationVersionForLanguageAsync(
        Guid languageId,
        IReadOnlyDictionary<string, string> entries,
        string createdBy = "resx-assemblies-import")
    {
        if (entries.Count == 0) return;
        
        var keys = entries.Keys.Where(k => !string.IsNullOrWhiteSpace(k)).Distinct().ToList();
        if (keys.Count == 0) return;
        
        // 1) Ensure UIResourceKeys exist
        var keyMap = await _db.UIResourceKeys
            .Where(r => keys.Contains(r.ResourceKey))
            .ToDictionaryAsync(r => r.ResourceKey, r => r.Id);

        var missingKeys = keys.Where(k => !keyMap.ContainsKey(k)).ToList();
        if (missingKeys.Count > 0)
        {
            await _db.UIResourceKeys.AddRangeAsync(
                missingKeys.Select(k => new UIResourceKeys { ResourceKey = k }));
            await _db.SaveChangesAsync();

            keyMap = await _db.UIResourceKeys
                .Where(r => keys.Contains(r.ResourceKey))
                .ToDictionaryAsync(r => r.ResourceKey, r => r.Id);
        }
        
        // 2) Insert missing (language, key) pairs as version 1
        var resourceKeysIds = keyMap.Values.ToList();
        var existingPairs = await _db.UITranslationVersions
            .Where(v => v.LanguageId == languageId && resourceKeysIds.Contains(v.ResourceKeyId))
            .Select(v => v.ResourceKeyId)
            .Distinct()
            .ToListAsync();

        var missingPairIds = resourceKeysIds.Except(existingPairs).ToHashSet();
        if (missingPairIds.Count == 0) return;

        var now = DateTime.UtcNow;
        var versionsToInsert = new List<UITranslationVersions>(missingPairIds.Count);

        foreach (var (key, content) in entries)
        {
            if (!keyMap.TryGetValue(key, out var resourceKeyId)) continue;
            if (!missingPairIds.Contains(resourceKeyId)) continue;

            versionsToInsert.Add(new UITranslationVersions
            {
                LanguageId = languageId,
                ResourceKeyId = resourceKeyId,
                VersionNumber = 1,
                Content = content ?? string.Empty,
                TranslationState = TranslationState.Published,
                CreatedAt = now,
                CreatedBy = createdBy
            });
        }

        if (versionsToInsert.Count == 0) return;

        await _db.UITranslationVersions.AddRangeAsync(versionsToInsert);
        await _db.SaveChangesAsync();
    }

    public async Task<int> InialUITranslationsImportAsync(string publishedBy)
    {
        var v1PerPair = await _db.UITranslationVersions
            .Where(v => v.VersionNumber == 1 && v.TranslationState == TranslationState.Published)
            .GroupBy(v => new { v.LanguageId, v.ResourceKeyId })
            .Select(g => g.OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.LanguageId, x.ResourceKeyId })
                .First())
            .ToListAsync();

        if (v1PerPair.Count == 0) return 0;

        var existingPairs = await _db.UITranslations
            .Select(t => new { t.LanguageId, t.ResourceKeyId })
            .ToListAsync();

        var existingSet = new HashSet<(Guid Lang, Guid Key)>(
            existingPairs.Select(p => (p.LanguageId, p.ResourceKeyId)));

        var now = DateTime.UtcNow;
        var toInsert = new List<UITranslations>();

        foreach (var v in v1PerPair)
        {
            var pair = (v.LanguageId, v.ResourceKeyId);
            if (!existingSet.Add(pair)) continue; 

            toInsert.Add(new UITranslations
            {
                LanguageId = v.LanguageId,
                ResourceKeyId = v.ResourceKeyId,
                TranslationVersionId = v.Id,
                PublishedAt = now,
                PublishedBy = publishedBy
            });
        }

        if (toInsert.Count == 0) return 0;

        await _db.UITranslations.AddRangeAsync(toInsert);
        return await _db.SaveChangesAsync();
    }
}