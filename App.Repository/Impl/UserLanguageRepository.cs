using App.Domain.Identity;
using App.EF;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class UserLanguageRepository : IUserLanguageRepository
{
    private readonly AppDbContext _db;
    
    public UserLanguageRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<IReadOnlyList<Guid>> GetLanguageIdsByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.UserLanguages
            .Where(x => x.UserId == userId)
            .Select(x => x.LanguageId)
            .ToListAsync(ct);
    }

    public async Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languagesIds, CancellationToken ct = default)
    {
        var selectedLanguages = languagesIds?.Distinct().ToHashSet() ?? new HashSet<Guid>();
            
        var existingLanguages = await _db.UserLanguages
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        // Remove Unselected Languages
        var toRemove = existingLanguages.Where(x => !selectedLanguages.Contains(x.LanguageId)).ToList();
        _db.UserLanguages.RemoveRange(toRemove);
        
        // Add new User Languages
        var existingIds = existingLanguages.Select(x => x.LanguageId).ToHashSet();
        var toAdd = selectedLanguages.Except(existingIds).Select(id => new UserLanguages
        {
            UserId = userId,
            LanguageId = id
        });

        await _db.UserLanguages.AddRangeAsync(toAdd,ct);
        await _db.SaveChangesAsync(ct);
    }
}