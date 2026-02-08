using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using WebApp.Extensions.Pager;
using WebApp.Extensions.Pager.models;

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

        var result = _db.UITranslations
            .AsNoTracking()
            .Where(t => !languageId.HasValue || t.LanguageId == languageId.Value)
            .OrderBy(t => t.Language.LanguageTag)
            .ThenBy(t => t.UIResourceKeys.FriendlyKey)
            .Select(t => new LiveTranslationDto(
                t.LanguageId,
                t.Language.LanguageTag,
                t.ResourceKeyId,
                t.UIResourceKeys.ResourceKey,
                t.UIResourceKeys.FriendlyKey,
                t.TranslationVersionId,
                t.UITranslationVersions.VersionNumber,
                t.UITranslationVersions.Content,
                t.UITranslationVersions.TranslationState
            ));

        return await result.ToListAsync();
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

    public async Task<PagedResult<FilteredUITranslationsDto>> GetFilteredUITranslationsAsync(FilteredTranslationsRequestDto request, PagedRequest paging)
    {
        paging = paging.Normalize();

        var query = _db.UITranslationVersions.AsNoTracking()
            .Where(x => x.LanguageId == request.LanguageId);

        if (request.VersionNumber.HasValue)
            query = query.Where(x => x.VersionNumber == request.VersionNumber.Value);

        if (request.State.HasValue)
            query = query.Where(x => x.TranslationState == request.State.Value);

        var dataQuery = query
            .OrderBy(x => x.UIResourceKeys.ResourceKey)
            .Select(x => new FilteredUITranslationsDto(
                x.LanguageId,
                x.Language.LanguageTag,
                x.ResourceKeyId,
                x.UIResourceKeys.ResourceKey,
                x.UIResourceKeys.FriendlyKey,
                x.Id,
                x.VersionNumber,
                x.Content,
                x.TranslationState
            ));

        return await dataQuery.ToPagedResultAsync(paging);
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
}