using System.Globalization;
using System.Text.RegularExpressions;
using App.EF;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class ResxImportRepository : IResxImportRepository
{
    private readonly AppDbContext _db;
    
    public ResxImportRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
    
    // https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-resources-resourcereader
    // https://learn.microsoft.com/en-us/dotnet/api/system.resources.resxresourcereader?view=windowsdesktop-9.0
    // https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-9.0
    public async Task ImportAsync(string resxFolder)
    {
        var languages = await _db.Languages
            .Select(p => new
            {
                p.Id, 
                p.LanguageTag
            })
            .ToListAsync();

        var langTag = languages
            .ToDictionary(
                x => CultureInfo.GetCultureInfo(x.LanguageTag).Name,
                x => x.Id
            );

        var resxFiles = Directory
            .EnumerateFiles(resxFolder, "*.resx", SearchOption.AllDirectories)
            .ToList();
        
        var groups = resxFiles.GroupBy(GetResourceFileBaseName);

        foreach (var group in groups)
        {
            // Import in db
            await ImportResourceAsync(group.ToList(), langTag);
        }
    }

    private async Task ImportResourceAsync(List<string> resxFiles, Dictionary<string, Guid> languageTags)
    {
        foreach (var file in resxFiles)
        {
            var (culture, isDefault) = GetCultureFromFileName(file);
            var normalizedCultureName = CultureInfo.GetCultureInfo(culture).Name;
            
            // Perform extra check if language exists
            
            using var reader = new ResXResourceReader(file) { UseResXDataNodes = true };
        }
    }
    
    // Helper
    private static string GetResourceFileBaseName(string fileName)
    {
        var parts = fileName.Split(".");
        return parts[0];
    }
    
    private static (string culture, bool isDefault) GetCultureFromFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var parts = name.Split(".");
        if (parts.Length == 1) return (parts[0], true);
        
        return (parts[^1], false);
    }
}