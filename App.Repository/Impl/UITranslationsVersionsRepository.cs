using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.DTO;
using App.Repository.Interface;
using App.Repository.Pager;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class UITranslationsVersionsRepository : IUITranslationsVersionsRepository
{
    private readonly AppDbContext _db;

    private sealed record KeyRow(Guid Id, string ResourceKey, string FriendlyKey);
    
    public UITranslationsVersionsRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<PagedResult<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(
        Guid? languageId, 
        PagedRequest paging,
        string? keySearch = null)
    {
        paging = paging.Normalize();
        
        if (languageId is null || languageId == Guid.Empty)
            return EmptyPaged<TranslationVersionRowDto>(paging);

        var languageTag = await _db.Languages
            .AsNoTracking()
            .Where(l => l.Id == languageId)
            .Select(l => l.LanguageTag)
            .SingleAsync();

        IQueryable<UIResourceKeys> keysQuery = _db.UIResourceKeys.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keySearch))
        {
            keysQuery = keysQuery.Where(k =>
                k.ResourceKey.Contains(keySearch) ||
                k.FriendlyKey.Contains(keySearch));
        }

        var pagedKeys = await keysQuery
            .OrderBy(k => k.ResourceKey)
            .Select(k => new KeyRow(k.Id, k.ResourceKey, k.FriendlyKey))
            .ToPagedResultAsync(paging);

        if (pagedKeys.Items.Count == 0)
        {
            return new PagedResult<TranslationVersionRowDto>
            {
                Items = Array.Empty<TranslationVersionRowDto>(),
                TotalCount = pagedKeys.TotalCount,
                Page = pagedKeys.Page,
                PageSize = pagedKeys.PageSize
            };
        }

        var pageKeyIds = pagedKeys.Items.Select(k => k.Id).ToArray();

        var latest = await _db.UITranslationVersions
            .AsNoTracking()
            .Where(v => v.LanguageId == languageId && pageKeyIds.Contains(v.ResourceKeyId))
            .GroupBy(v => v.ResourceKeyId)
            .Select(g => g
                .OrderByDescending(v => v.VersionNumber)
                .ThenByDescending(v => v.CreatedAt)
                .First())
            .ToDictionaryAsync(v => v.ResourceKeyId);

        var rows = pagedKeys.Items.Select(k =>
        {
            latest.TryGetValue(k.Id, out var v);
            return new TranslationVersionRowDto(
                k.Id,
                k.ResourceKey,
                k.FriendlyKey,
                v?.Content,
                languageId,
                languageTag,
                v?.VersionNumber
            );
        }).ToList();
        

        return new PagedResult<TranslationVersionRowDto>
        {
            Items = rows,
            TotalCount = pagedKeys.TotalCount,
            Page = pagedKeys.Page,
            PageSize = pagedKeys.PageSize
        };
    }

    public async Task<PagedResult<TranslationVersionRowDto>> GetTranslationVersionAsync(
        Guid? languageId, 
        int? version,
        PagedRequest paging, 
        string? keySearch = null)
    {
        paging = paging.Normalize();
        
        if (languageId is null || languageId == Guid.Empty)
            return EmptyPaged<TranslationVersionRowDto>(paging);

        var langTag = await _db.Languages
            .AsNoTracking()
            .Where(l => l.Id == languageId)
            .Select(l => l.LanguageTag)
            .SingleAsync();

        IQueryable<UIResourceKeys> keysQuery = _db.UIResourceKeys.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keySearch))
        {
            keysQuery = keysQuery.Where(k =>
                k.ResourceKey.Contains(keySearch) ||
                k.FriendlyKey.Contains(keySearch));
        }

        var pagedKeys = await keysQuery
            .OrderBy(k => k.ResourceKey)
            .Select(k => new KeyRow(k.Id, k.ResourceKey, k.FriendlyKey))
            .ToPagedResultAsync(paging);

        if (pagedKeys.Items.Count == 0)
        {
            return new PagedResult<TranslationVersionRowDto>
            {
                Items = Array.Empty<TranslationVersionRowDto>(),
                TotalCount = pagedKeys.TotalCount,
                Page = pagedKeys.Page,
                PageSize = pagedKeys.PageSize
            };
        }

        var pageKeyIds = pagedKeys.Items.Select(k => k.Id).ToArray();

        var versionsQuery = _db.UITranslationVersions
            .AsNoTracking()
            .Where(v => v.LanguageId == languageId && pageKeyIds.Contains(v.ResourceKeyId));

        Dictionary<Guid, UITranslationVersions> byKey;

        if (version is null)
        {
            byKey = await versionsQuery
                .GroupBy(v => v.ResourceKeyId)
                .Select(g => g
                    .OrderByDescending(v => v.VersionNumber)
                    .ThenByDescending(v => v.CreatedAt)
                    .First())
                .ToDictionaryAsync(v => v.ResourceKeyId);
        }
        else
        {
            byKey = await versionsQuery
                .Where(v => v.VersionNumber == version.Value)
                .ToDictionaryAsync(v => v.ResourceKeyId);
        }

        var rows = pagedKeys.Items.Select(k =>
        {
            byKey.TryGetValue(k.Id, out var v);

            return new TranslationVersionRowDto(
                k.Id,
                k.ResourceKey,
                k.FriendlyKey,
                v?.Content,
                languageId,
                langTag,
                v?.VersionNumber
            );
        }).ToList();

        return new PagedResult<TranslationVersionRowDto>
        {
            Items = rows,
            TotalCount = pagedKeys.TotalCount,
            Page = pagedKeys.Page,
            PageSize = pagedKeys.PageSize
        };
    }

    public async Task<int> CreateNewVersionAsync(CreateVersionRequestDto request)
    {
        if (request.ResourceKeyIds == null) return 0;

        var keyIds = request.ResourceKeyIds.Distinct().ToArray();

        // For each key, get the latest version row so we can decide:
        // - Rejected latest => reuse same version and move it back to WaitingReview
        // - Anything else => create a brand-new version (VersionNumber + 1)
        var latestPerKey = await _db.UITranslationVersions
            .Where(v => v.LanguageId == request.LanguageId && keyIds.Contains(v.ResourceKeyId))
            .GroupBy(v => v.ResourceKeyId)
            .Select(g => g
                .OrderByDescending(v => v.VersionNumber)
                .ThenByDescending(v => v.CreatedAt)
                .First())
            .ToDictionaryAsync(v => v.ResourceKeyId);

        var toInsert = new List<UITranslationVersions>();
        var updatedRejected = new List<(UITranslationVersions Version, TranslationState OldState, string OldContent)>();

        foreach (var keyId in keyIds)
        {
            request.Content.TryGetValue(keyId, out var content);
            content ??= string.Empty;

            if (latestPerKey.TryGetValue(keyId, out var latest) &&
                latest.TranslationState == TranslationState.Rejected)
            {
                var oldState = latest.TranslationState;
                var oldContent = latest.Content;

                latest.Content = content;
                latest.TranslationState = TranslationState.WaitingReview;
                latest.CreatedAt = DateTime.UtcNow;
                latest.CreatedBy = request.CreatedBy;

                updatedRejected.Add((latest, oldState, oldContent));
                continue;
            }

            var nextVersion = latestPerKey.TryGetValue(keyId, out var current) ? current.VersionNumber + 1 : 0;
            var versionId = Guid.NewGuid();
            
            toInsert.Add(new UITranslationVersions
            {
                Id = versionId,
                LanguageId = request.LanguageId,
                ResourceKeyId = keyId,
                VersionNumber = nextVersion,
                Content = content,
                TranslationState = TranslationState.WaitingReview,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy
            });
        }

        await _db.UITranslationVersions.AddRangeAsync(toInsert);
        var now = DateTime.UtcNow;
        
        var createdAuditRows = toInsert.Select(v => new UITranslationAuditLog
        {
            LanguageId = v.LanguageId,
            ResourceKeyId = v.ResourceKeyId,
            TranslationVersionId = v.Id,
            ActivatedAt = now,
            ActivatedBy = request.CreatedBy,
            DeactivatedAt = now,
            DeactivatedBy = request.CreatedBy,
            ActionType = TranslationAuditAction.VersionCreated,
            ChangedAt = now,
            ChangedBy = request.CreatedBy,
            OldState = null,
            NewState = v.TranslationState,
            OldContent = null,
            NewContent = v.Content
        }).ToList();
        
        var revisedAuditRows = updatedRejected.Select(x => new UITranslationAuditLog
        {
            LanguageId = x.Version.LanguageId,
            ResourceKeyId = x.Version.ResourceKeyId,
            TranslationVersionId = x.Version.Id,
            ActivatedAt = now,
            ActivatedBy = request.CreatedBy,
            DeactivatedAt = now,
            DeactivatedBy = request.CreatedBy,
            ActionType = TranslationAuditAction.StateChanged,
            ChangedAt = now,
            ChangedBy = request.CreatedBy,
            OldState = x.OldState,
            NewState = TranslationState.WaitingReview,
            OldContent = x.OldContent,
            NewContent = x.Version.Content
        }).ToList();

        await _db.UITranslationAuditLogs.AddRangeAsync(createdAuditRows);
        await _db.UITranslationAuditLogs.AddRangeAsync(revisedAuditRows);
        return await _db.SaveChangesAsync();
    }

    private static PagedResult<T> EmptyPaged<T>(PagedRequest paging) => new()
    {
        Items = Array.Empty<T>(),
        TotalCount = 0,
        Page = paging.Page,
        PageSize = paging.PageSize
    };
}
