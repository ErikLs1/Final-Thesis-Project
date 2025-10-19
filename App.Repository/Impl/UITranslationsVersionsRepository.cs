using App.Domain.Enum;
using App.Domain.UITranslationEntities;
using App.EF;
using App.Repository.DTO;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class UITranslationsVersionsRepository : IUITranslationsVersionsRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationsVersionsRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(Guid? languageId, CancellationToken ct = default)
    {
        var languageTag = await _db.Languages
            .Where(l => l.Id == languageId)
            .Select(l => l.LanguageTag)
            .SingleAsync(ct);
        
        // Get all ResourceKeys
        var keys = await _db.UIResourceKeys
            .AsNoTracking()
            .Select(rk => new { rk.Id, rk.ResourceKey })
            .OrderBy(rk => rk.ResourceKey)
            .ToListAsync(ct);
        
        // Get latest version per key
        var latestRows = await _db.UITranslationVersions
            .AsNoTracking()
            .Where(v => v.LanguageId == languageId)
            .GroupBy(v => v.ResourceKeyId)
            .Select(g => g
                .OrderByDescending(v => v.VersionNumber)
                .ThenByDescending(v => v.CreatedAt)
                .First())
            .ToDictionaryAsync(v => v.ResourceKeyId, ct);
        
        var rows = keys.Select(k =>
        {
            latestRows.TryGetValue(k.Id, out var v);
            return new TranslationVersionRowDto(
                k.Id,
                k.ResourceKey,
                ConvertToFriendlyString(k.ResourceKey),
                v?.Content,
                languageId,
                languageTag,
                v?.VersionNumber
            );
        }).ToList();

        return rows;
    }

    public async Task<IReadOnlyList<TranslationVersionRowDto>> GetTranslationVersionAsync(Guid? languageId, int? version, CancellationToken ct = default)
    {
        var langTag = await _db.Languages.Where(l => l.Id == languageId).Select(l => l.LanguageTag).SingleAsync(ct);

        // 1) Get All Keys
        var keys = await _db.UIResourceKeys
            .AsNoTracking()
            .Select(rk => new { rk.Id, rk.ResourceKey })
            .OrderBy(rk => rk.ResourceKey)
            .ToListAsync(ct);

        // 2) pick versions into a dictionary<ResourceKeyId, VersionRow>
        Dictionary<Guid, UITranslationVersions> byKey;
        
        
        if (version is null)
        {
            // latest per key
            byKey = await _db.UITranslationVersions
                .AsNoTracking()
                .Where(v => v.LanguageId == languageId)
                .GroupBy(v => v.ResourceKeyId)
                .Select(g => g
                    .OrderByDescending(v => v.VersionNumber)
                    .ThenByDescending(v => v.CreatedAt)
                    .First())
                .ToDictionaryAsync(v => v.ResourceKeyId, ct);
        }
        else
        {
            // specific version
            byKey = await _db.UITranslationVersions
                .AsNoTracking()
                .Where(v => v.LanguageId == languageId && v.VersionNumber == version.Value)
                .ToDictionaryAsync(v => v.ResourceKeyId, ct);
        }

        // 3) project in-memory
        var rows = keys.Select(k =>
        {
            byKey.TryGetValue(k.Id, out var v);
            return new TranslationVersionRowDto(
                k.Id,
                k.ResourceKey,
                ConvertToFriendlyString(k.ResourceKey),
                v?.Content,
                languageId,
                langTag,
                v?.VersionNumber
            );
        }).ToList();

        return rows;
    }

    public async Task<int> CreateNewVersionAsync(CreateVersionRequestDto request, CancellationToken ct = default)
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

        await _db.UITranslationVersions.AddRangeAsync(toInsert, ct);
        return await _db.SaveChangesAsync(ct);
    }

    private static string ConvertToFriendlyString(string resourceKey)
    {
        return string.Join(" ", resourceKey.Split('_', StringSplitOptions.RemoveEmptyEntries));
    }
}