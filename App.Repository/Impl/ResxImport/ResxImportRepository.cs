using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.Resources.NetStandard;
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
    
    
    public async Task ImportFirstTranslationVersionAsync(string resxFolder)
    {
        // Make sure folder exists
        if (!Directory.Exists(resxFolder))
            throw new DirectoryNotFoundException($"Folder not found: {resxFolder}");

        // Find all *.resx files in given folder
        var resxFiles = Directory.EnumerateFiles(resxFolder, "*.resx", SearchOption.AllDirectories).ToList();
        if (resxFiles.Count == 0)
            throw new FileNotFoundException($"No .resx files found in {resxFolder}");
        
        // Map language tag and id
        var languages = await _db.Languages
            .Select(l => new { l.Id, l.LanguageTag, l.IsDefaultLanguage })
            .ToListAsync();
        
        var langByTag = languages.ToDictionary(x => x.LanguageTag, x => x.Id);
        var defaultLangId = languages
            .Where(x => x.IsDefaultLanguage)
            .Select(x => x.Id)
            .SingleOrDefault();

        // Parse files
        var localizedFiles = resxFiles.Where(f => !IsDefaultResx(f)).ToList();
        var neutralFiles = resxFiles.Where(IsDefaultResx).ToList();

        foreach (var file in localizedFiles)
            await ImportFileContentAsync(file, langByTag, null);
        
        foreach (var file in neutralFiles)
            await ImportFileContentAsync(file, langByTag, defaultLangId);
        
    }

    private async Task ImportFileContentAsync(string file, Dictionary<string, Guid> langByTag, Guid? defaultLangId)
    {
        var (culture, isDefault) = GetCultureFromFileName(file);

        Guid languageId;
        if (isDefault)
        {
            languageId = defaultLangId!.Value;
        }
        else
        {
            var tag = NormalizeTag(culture);
            if (!langByTag.TryGetValue(tag, out languageId)) return;
        }

        var fileEntries = ReadResxEntries(file);
        
        if (fileEntries.Count == 0) return;

        var keys = fileEntries.Keys.ToList();
        var keyMap = await _db.UIResourceKeys
            .Where(r => keys.Contains(r.ResourceKey))
            .ToDictionaryAsync(r => r.ResourceKey, r => r.Id);

        var missingKeys = keys.Where(k => !keyMap.ContainsKey(k)).ToList();
        if (missingKeys.Count > 0)
        {
            await _db.UIResourceKeys.AddRangeAsync(missingKeys.Select(k => new UIResourceKeys { ResourceKey = k }));
            await _db.SaveChangesAsync();

            keyMap = await _db.UIResourceKeys
                .Where(r => keys.Contains(r.ResourceKey))
                .ToDictionaryAsync(r => r.ResourceKey, r => r.Id);
        }
        
        var resourceKeysIds = keyMap.Values.ToList();
        var existingPairs = await _db.UITranslationVersions
            .Where(v => v.LanguageId == languageId && resourceKeysIds.Contains(v.ResourceKeyId))
            .Select(v => v.ResourceKeyId)
            .Distinct()
            .ToListAsync();
        
        var missingPairIds = resourceKeysIds.Except(existingPairs).ToHashSet();
        
        if (missingPairIds.Count == 0) return;
        
        var versionsToInsert = new List<UITranslationVersions>();
        
        foreach (var (key, content) in fileEntries)
        {
            var resourceKeyId = keyMap[key];
            if (!missingPairIds.Contains(resourceKeyId)) continue;
            
            versionsToInsert.Add(new UITranslationVersions
            {
                LanguageId = languageId,
                ResourceKeyId = resourceKeyId,
                VersionNumber = 1,
                Content = content,
                TranslationState = TranslationState.Published,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "resx-import"
            });
        }
        
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
    
    // HELPERS
    private static Dictionary<string, string> ReadResxEntries(string file)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        using var reader = new ResXResourceReader(file) { UseResXDataNodes = true };

        foreach (DictionaryEntry entry in reader)
        {
            var key = entry.Key?.ToString();
            if (string.IsNullOrWhiteSpace(key)) continue;

            var node = (ResXDataNode)entry.Value!;
            var value = node.GetValue((ITypeResolutionService?)null)?.ToString() ?? string.Empty;
            dict[key!] = value;
        }
        return dict;
    }
    private static bool IsDefaultResx(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1;
    }

    private static (string culture, bool isNeutral) GetCultureFromFileName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return ("", true);
        return (parts[^1], false);
    }

    private static string NormalizeTag(string tag) // Change later
    {
        if (string.IsNullOrWhiteSpace(tag)) return tag;
        try
        {
            return CultureInfo.GetCultureInfo(tag).Name;
        }
        catch
        {
            return tag;
        }
    }
}