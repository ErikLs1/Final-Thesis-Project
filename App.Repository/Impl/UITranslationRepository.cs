using App.EF;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class UITranslationRepository : IUITranslationRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId)
    {
        var live = await _db.UITranslations
            .Where(t => !languageId.HasValue || t.LanguageId == languageId.Value)
            .Select(t => new { t.LanguageId, t.ResourceKeyId, t.TranslationVersionId, t.PublishedAt, t.PublishedBy })
            .ToListAsync();

        if (live.Count == 0) return Array.Empty<LiveTranslationDto>();

        var langIds = live.Select(x => x.LanguageId)
            .Distinct()
            .ToList();
        
        var keyIds = live.Select(x => x.ResourceKeyId)
            .Distinct()
            .ToList();
        
        var versionIds = live.Select(x => x.TranslationVersionId)
            .Distinct()
            .ToList();
        
        var langMap = await _db.Languages
            .Where(l => langIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.LanguageTag);

        var keyMap = await _db.UIResourceKeys
            .Where(rk => keyIds.Contains(rk.Id))
            .ToDictionaryAsync(rk => rk.Id, rk => rk.ResourceKey);

        var verMap = await _db.UITranslationVersions
            .Where(v => versionIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, v => new { v.VersionNumber, v.Content, v.TranslationState });
        
        var rows = live
            .Select(x =>
            {
                var langTag = langMap.TryGetValue(x.LanguageId, out var lt) ? lt : "";
                var keyStr  = keyMap.TryGetValue(x.ResourceKeyId, out var ks) ? ks : "";
                var frKey   = ConvertToUserFriendlyString(keyStr);

                verMap.TryGetValue(x.TranslationVersionId, out var vinfo);

                return new LiveTranslationDto(
                    x.LanguageId,
                    langTag,
                    x.ResourceKeyId,
                    keyStr,
                    frKey,
                    x.TranslationVersionId,
                    vinfo?.VersionNumber ?? 0,
                    vinfo?.Content ?? "",
                    vinfo?.TranslationState ?? default
                );
            })
            .OrderBy(r => r.LanguageTag)
            .ThenBy(r => r.FriendlyKey)
            .ToList();

        return rows;
    }

    public async Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request)
    {
        var version = await _db.UITranslationVersions
            .FirstOrDefaultAsync(v => v.Id == request.TranslationVersionId);

        if (version == null) return 0;
        
        version.TranslationState = request.NewState;

        return await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag)
    {
        var langId = await _db.Languages
            .Where(l => l.LanguageTag == languageTag)
            .Select(l => l.Id)
            .SingleAsync();
        
        var query =
            from t in _db.UITranslations.AsNoTracking()
            where t.LanguageId == langId
            join rk in _db.UIResourceKeys.AsNoTracking() on t.ResourceKeyId equals rk.Id
            join v  in _db.UITranslationVersions.AsNoTracking() on t.TranslationVersionId equals v.Id
            select new { rk.ResourceKey, v.Content };

        return await query.ToDictionaryAsync(x => x.ResourceKey, x => x.Content);
    }

    public async Task<IReadOnlyList<FilteredUITranslationsDto>> GetFilteredUITranslationsAsync(FilteredTranslationsRequestDto request)
    {
        var translations = await _db.UITranslationVersions
            .Where(x => x.LanguageId == request.LanguageId)
            .Select(x => new
            {
                x.LanguageId,
                x.Language.LanguageTag,
                x.ResourceKeyId,
                x.UIResourceKeys.ResourceKey,
                x.Id,
                x.VersionNumber,
                x.Content,
                x.TranslationState
            })
            .ToListAsync();

        if (translations.Count == 0)
        {
            return [];
        }

        if (request.VersionNumber.HasValue)
        {
            translations = translations
                .Where(x => x.VersionNumber == request.VersionNumber.Value)
                .ToList();
        }
        
        if (request.State.HasValue)
        {
            translations = translations
                .Where(x => x.TranslationState == request.State.Value)
                .ToList();
        }

        var result = new List<FilteredUITranslationsDto>();
        
        foreach (var x in translations)
        {
            var friendly = ConvertToUserFriendlyString(x.ResourceKey ?? string.Empty);

            result.Add(new FilteredUITranslationsDto(
                x.LanguageId,
                x.LanguageTag ?? string.Empty,
                x.ResourceKeyId,
                x.ResourceKey ?? string.Empty,
                friendly,
                x.Id,
                x.VersionNumber,
                x.Content ?? string.Empty,
                x.TranslationState
            ));
        }

        return result;
    }

    private static string ConvertToUserFriendlyString(string key)
    {
        return string.Join(" ", key.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}