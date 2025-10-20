using App.EF;
using App.Repository.DTO;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _db;
    
    public LanguageRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<IReadOnlyList<LanguageDto>> GetAllLanguagesAsync()
    {
        return await _db.Languages
            .Select(l => new LanguageDto(l.Id, l.LanguageTag, l.LanguageName))
            .ToListAsync();
    }

    public async Task<Guid> GetDefaultLanguageIdAsync()
    {
        return await _db.Languages
            .Where(l => l.IsDefaultLanguage)
            .Select(l => l.Id)
            .SingleOrDefaultAsync();
    }
}