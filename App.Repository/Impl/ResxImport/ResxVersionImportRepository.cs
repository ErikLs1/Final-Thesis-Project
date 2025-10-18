using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.Resources.NetStandard;
using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Repository.Impl.ResxImport;

public class ResxVersionImportRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ResxVersionImportRepository> _logger;

    public ResxVersionImportRepository(AppDbContext db, ILogger<ResxVersionImportRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ImportFirstTranslationVersionAsync(string resxFolder, CancellationToken ct = default)
    {
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
        var defaultLangId = languages.Where(x => x.IsDefaultLanguage).Select(x => x.Id).SingleOrDefault();

        // Parse files
        var localizedFiles = resxFiles.Where(f => !IsNeutralResx(f)).ToList();
        var neutralFiles = resxFiles.Where(IsNeutralResx).ToList();

        foreach (var file in localizedFiles)
            await ImportFileContentAsync(file, langByTag, null, ct);
        
        foreach (var file in neutralFiles)
            await ImportFileContentAsync(file, langByTag, defaultLangId, ct);
        
        _logger.LogInformation("Translations import completed.");
    }

    private async Task ImportFileContentAsync(string file, Dictionary<string, Guid> langByTag, Guid? defaultLangId, CancellationToken ct)
    {
        var (culture, isNeutral) = GetCultureFromFileName(file);

        // TODO: CHANGE LATER
        Guid languageId;
        if (isNeutral)
        {
            languageId = defaultLangId!.Value;
        }
        else
        {
            var tag = NormalizeTag(culture);
            if (!langByTag.TryGetValue(tag, out languageId))
            {
                _logger.LogWarning("Cannot get language tag  for file {File}. No language in DB for tag {Tag}.", Path.GetFileName(file), tag);
                return;
            }
        }

        var fileEntries = ReadResxEntries(file);
        if (fileEntries.Count == 0)
        {
            _logger.LogInformation("No entries in {File}.", Path.GetFileName(file));
            return;
        }

        // Ensure keys exists
        var keys = fileEntries.Keys.ToList();
        var keyMap = await _db.UIResourceKeys
            .Where(r => keys.Contains(r.ResourceKey))
            .ToDictionaryAsync(r => r.ResourceKey, r => r.Id, ct);

        var missingKeys = keys.Where(k => !keyMap.ContainsKey(k)).ToList();
        if (missingKeys.Count > 0)
        {
            await _db.UIResourceKeys.AddRangeAsync(missingKeys.Select(k => new UIResourceKeys { ResourceKey = k }), ct);
            await _db.SaveChangesAsync(ct);

            keyMap = await _db.UIResourceKeys
                .Where(r => keys.Contains(r.ResourceKey))
                .ToDictionaryAsync(r => r.ResourceKey, r => r.Id, ct);
        }
        
        // Find which language and resource key already have the key
        var resourceKeysIds = keyMap.Values.ToList();
        var existingPairs = await _db.UITranslationVersions
            .Where(v => v.LanguageId == languageId && resourceKeysIds.Contains(v.ResourceKeyId))
            .Select(v => v.ResourceKeyId)
            .Distinct()
            .ToListAsync(ct);
        
        var missingPairIds = resourceKeysIds.Except(existingPairs).ToHashSet();
        
        if (missingPairIds.Count == 0)
        {
            _logger.LogInformation("All keys in {File} already have first translation version. Nothing to insert.", Path.GetFileName(file));
            return;
        }
        
        // Insert Versions
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
        
        if (versionsToInsert.Count > 0)
        {
            await _db.UITranslationVersions.AddRangeAsync(versionsToInsert, ct);
            var written = await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Inserted {Count} version rows from {File}.", written, Path.GetFileName(file));
        }
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
    private static bool IsNeutralResx(string path)
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