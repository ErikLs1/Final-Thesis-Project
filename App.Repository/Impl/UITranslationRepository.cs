using App.Domain.Enum;
using App.Domain.UITranslationEntities;
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

        var liveTranslations = await _db.UITranslations
            .Where(x => x.LanguageId == langId)
            .Select(x => new
            {
                x.UIResourceKeys.ResourceKey,
                x.UITranslationVersions.Content
            }).ToDictionaryAsync(
                x => x.ResourceKey,
                x => x.Content
            );

        return liveTranslations;
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
            var friendly = ConvertToUserFriendlyString(x.ResourceKey);

            result.Add(new FilteredUITranslationsDto(
                x.LanguageId,
                x.LanguageTag,
                x.ResourceKeyId,
                x.ResourceKey,
                friendly,
                x.Id,
                x.VersionNumber,
                x.Content,
                x.TranslationState
            ));
        }

        return result;
    }

    public async Task<int> PublishTranslationVersionAsync(PublishTranslationVersionRequestDto request)
    {
        var newTranslationVersion = await _db.UITranslationVersions
            .FirstOrDefaultAsync(x => x.Id == request.TranslationVersionId);

        if (newTranslationVersion == null)
        {
            return 0;
        }
        
        // Currently published translations
        var publishedTranslations = await _db.UITranslations
            .FirstOrDefaultAsync(x =>
                x.LanguageId == newTranslationVersion.LanguageId &&
                x.ResourceKeyId == newTranslationVersion.ResourceKeyId);

        UITranslationVersions? current = null; 

        if (publishedTranslations != null && publishedTranslations.TranslationVersionId != newTranslationVersion.Id)
        {
            current = await _db.UITranslationVersions
                .FirstOrDefaultAsync(x => x.Id == publishedTranslations.TranslationVersionId);

            var audit = new UITranslationAuditLog
            {
                LanguageId = publishedTranslations.LanguageId,
                ResourceKeyId = publishedTranslations.ResourceKeyId,
                TranslationVersionId = publishedTranslations.TranslationVersionId,
                ActivatedAt = publishedTranslations.PublishedAt, // Todo redactor (delete this from uitranslation table)
                ActivatedBy = publishedTranslations.PublishedBy,
                DeactivatedAt = DateTime.UtcNow,
                DeactivatedBy = request.ActivatedBy
            };

            await _db.UITranslationAuditLogs.AddAsync(audit);

            if (current != null)
            {
                current.TranslationState = TranslationState.Inactive;
            }
        }


        newTranslationVersion.TranslationState = TranslationState.Published;
        
        if (publishedTranslations != null)
        {
            publishedTranslations.TranslationVersionId = newTranslationVersion.Id;
            publishedTranslations.PublishedAt = DateTime.UtcNow;
            publishedTranslations.PublishedBy = request.ActivatedBy;
        }
        
        return await _db.SaveChangesAsync();
    }

    private static string ConvertToUserFriendlyString(string key)
    {
        return string.Join(" ", key.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}