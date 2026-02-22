using App.EF;
using App.Repository.DTO.UITranslations;
using App.Repository.Interface;
using App.Repository.Pager;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class UITranslationAuditLogRepository: IUITranslationAuditLogRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationAuditLogRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task<PagedResult<TranslationAuditRowDto>> GetAuditLogsAsync(
        FilteredTranslationAuditRequestDto request,
        PagedRequest paging)
    {
        paging = paging.Normalize();

        var query = _db.UITranslationAuditLogs
            .AsNoTracking();

        if (request.LanguageId.HasValue)
            query = query.Where(x => x.LanguageId == request.LanguageId.Value);

        if (request.ActionType.HasValue)
            query = query.Where(x => x.ActionType == request.ActionType.Value);

        if (!string.IsNullOrWhiteSpace(request.ChangedBy))
            query = query.Where(x => x.ChangedBy.Contains(request.ChangedBy));

        if (!string.IsNullOrWhiteSpace(request.ResourceKeySearch))
        {
            query = query.Where(x =>
                x.UIResourceKeys.ResourceKey.Contains(request.ResourceKeySearch) ||
                x.UIResourceKeys.FriendlyKey.Contains(request.ResourceKeySearch));
        }

        if (request.ChangedFromUtc.HasValue)
            query = query.Where(x => x.ChangedAt >= request.ChangedFromUtc.Value);

        if (request.ChangedToUtc.HasValue)
            query = query.Where(x => x.ChangedAt <= request.ChangedToUtc.Value);

        var dataQuery = query
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new TranslationAuditRowDto(
                x.ChangedAt,
                x.ChangedBy,
                x.ActionType,
                x.LanguageId,
                x.Language.LanguageTag,
                x.ResourceKeyId,
                x.UIResourceKeys.ResourceKey,
                x.UIResourceKeys.FriendlyKey,
                x.TranslationVersionId,
                x.UITranslationVersions.VersionNumber,
                x.OldState,
                x.NewState,
                x.OldContent,
                x.NewContent
            ));

        return await dataQuery.ToPagedResultAsync(paging);
    }
}
