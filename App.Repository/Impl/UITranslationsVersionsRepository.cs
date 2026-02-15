using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.DTO;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using WebApp.Extensions.Pager;
using WebApp.Extensions.Pager.models;

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
        
        // Get current max version per resource key for the language
        var maxPerKey = await _db.UITranslationVersions
            .Where(v => v.LanguageId == request.LanguageId && request.ResourceKeyIds.Contains(v.ResourceKeyId))
            .GroupBy(v => v.ResourceKeyId)
            .Select(v => new { ResourceKeyId = v.Key, MaxVersion = v.Max(v => v.VersionNumber) })
            .ToDictionaryAsync(v => v.ResourceKeyId, x => x.MaxVersion);

        var toInsert = new List<UITranslationVersions>();

        foreach (var keyId in request.ResourceKeyIds)
        {
            var nextVersion = maxPerKey.TryGetValue(keyId, out var max) ? max + 1 : 0;

            request.Content.TryGetValue(keyId, out var content);
            content ??= string.Empty;
            
            toInsert.Add(new UITranslationVersions
            {
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
        return await _db.SaveChangesAsync();
    }

    private static string ConvertToFriendlyString(string resourceKey)
    {
        return string.Join(" ", resourceKey.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}