using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.Interface.ResxImport;
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
    
    public async Task ImportFirstTranslationVersionForLanguageAsync(Guid languageId, IReadOnlyDictionary<string, string> entries)
    {
        if (entries.Count == 0) return;

        // 1) Ensure resource keys exist and map to ids
        var keys = entries.Keys.Distinct().ToList();

        var existingKeys = await _db.UIResourceKeys
            .Where(k => keys.Contains(k.ResourceKey))
            .ToDictionaryAsync(k => k.ResourceKey, k => k.Id);

        var missingKeys = keys.Where(k => !existingKeys.ContainsKey(k)).ToList();
        if (missingKeys.Count > 0)
        {
            var newKeys = missingKeys.Select(k => new UIResourceKeys { ResourceKey = k }).ToList();
            await _db.UIResourceKeys.AddRangeAsync(newKeys);
            await _db.SaveChangesAsync();

            foreach (var nk in newKeys)
                existingKeys[nk.ResourceKey] = nk.Id;
        }

        var keyMap = existingKeys;
        
        // 2) Insert missing V1 translation versions for this language
        var resourceKeyIds = keyMap.Values.Distinct().ToList();

        var existingPairs = await _db.UITranslationVersions
            .Where(v => v.LanguageId == languageId && resourceKeyIds.Contains(v.ResourceKeyId))
            .Select(v => v.ResourceKeyId)
            .Distinct()
            .ToListAsync();

        var existingKeyIdSet = existingPairs.ToHashSet();
        var now = DateTime.UtcNow;

        var versionsToInsert = new List<UITranslationVersions>();

        foreach (var (key, value) in entries)
        {
            var resourceKeyId = keyMap[key];
            if (existingKeyIdSet.Contains(resourceKeyId)) continue;

            versionsToInsert.Add(new UITranslationVersions
            {
                LanguageId = languageId,
                ResourceKeyId = resourceKeyId,
                Content = value,
                VersionNumber = 1,
                TranslationState = TranslationState.Published,
                CreatedAt = now,
                CreatedBy = "resx-assemblies-import"
            });
            
            existingKeyIdSet.Add(resourceKeyId);
        }

        if (versionsToInsert.Count > 0)
        {
            await _db.UITranslationVersions.AddRangeAsync(versionsToInsert);
            await _db.SaveChangesAsync();
        }

        // 3) Ensure UITranslations ("live pointers") exist for this language only and only for the imported keys
        var importedKeyIds = keyMap.Values.Distinct().ToList();
        await ImportInitialUITranslationsForLanguageAsync(languageId, importedKeyIds);
    }
    
    private async Task<int> ImportInitialUITranslationsForLanguageAsync(
        Guid languageId,
        IReadOnlyCollection<Guid> resourceKeyIds)
    {
        if (resourceKeyIds.Count == 0) return 0;

        // Which keys already have live rows for this language?
        var existingKeyIds = await _db.UITranslations
            .Where(t => t.LanguageId == languageId && resourceKeyIds.Contains(t.ResourceKeyId))
            .Select(t => t.ResourceKeyId)
            .Distinct()
            .ToListAsync();

        var missingKeyIds = resourceKeyIds.Except(existingKeyIds).ToList();
        if (missingKeyIds.Count == 0) return 0;

        // Find v1 Published version id per key
        var v1PerKey = await _db.UITranslationVersions
            .Where(v => v.LanguageId == languageId
                        && missingKeyIds.Contains(v.ResourceKeyId)
                        && v.VersionNumber == 1
                        && v.TranslationState == TranslationState.Published)
            .GroupBy(v => v.ResourceKeyId)
            .Select(g => g.OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.ResourceKeyId })
                .First())
            .ToListAsync();

        if (v1PerKey.Count == 0) return 0;

        var now = DateTime.UtcNow;

        var toInsert = v1PerKey.Select(v => new UITranslations
        {
            LanguageId = languageId,
            ResourceKeyId = v.ResourceKeyId,
            TranslationVersionId = v.Id,
            PublishedAt = now,
            PublishedBy = "resx-startup-import"
        }).ToList();

        await _db.UITranslations.AddRangeAsync(toInsert);
        return await _db.SaveChangesAsync();
    }
}