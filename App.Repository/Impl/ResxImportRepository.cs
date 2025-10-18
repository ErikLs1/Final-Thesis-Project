using System.Collections;
using System.Resources.NetStandard;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Repository.Impl;

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

    /// <summary>
    /// Import all unique resource keys from .resx files into the database table - UI_RESOURCE_KEYS
    /// </summary>
    /// <param name="resxFolder"></param>
    /// <param name="ct"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task ImportKeysAsync(string resxFolder, CancellationToken ct = default)
    {
        if (!Directory.Exists(resxFolder))
            throw new DirectoryNotFoundException($"Folder not found: {resxFolder}");

        // Find all *.resx files in given folder
        var resxFiles = Directory.EnumerateFiles(resxFolder, "*.resx", SearchOption.AllDirectories).ToList();
        if (resxFiles.Count == 0)
            throw new FileNotFoundException($"No .resx files found in {resxFolder}");
        
        // Read the keys from the resx files
        var allKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in resxFiles)
        {
            using var reader = new ResXResourceReader(file) { UseResXDataNodes = true };

            foreach (DictionaryEntry entry in reader)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrWhiteSpace(key))
                    allKeys.Add(key!);
            }
        }

        if (allKeys.Count == 0)
            throw new KeyNotFoundException("No keys found in .resx files.");
        
        // Check which keys already exist in the database
        var existingKeys = await _db.UIResourceKeys
            .Where(r => allKeys.Contains(r.ResourceKey))
            .Select(r => r.ResourceKey)
            .ToListAsync();

        var missingKeys = allKeys.Except(existingKeys, StringComparer.OrdinalIgnoreCase).ToList();
        
        // Return if no new keys were created
        if (missingKeys.Count == 0)
        {
            _logger.LogInformation("All {Count} keys already exist database. Nothing new to insert.", existingKeys.Count);
            return;
        }
        
        // Insert missing keys
        var keysInsert = missingKeys.Select(k => new UIResourceKeys
        {
            ResourceKey = k
        }).ToList();

        await _db.UIResourceKeys.AddRangeAsync(keysInsert, ct);
        await _db.SaveChangesAsync(ct);
        
        _logger.LogInformation("Inserted {Count} new keys into the database.", missingKeys.Count);
    }
}