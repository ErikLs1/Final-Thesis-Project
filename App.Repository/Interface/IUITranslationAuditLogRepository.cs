using App.Repository.DTO.UITranslations;
using App.Repository.Pager;

namespace App.Repository.Interface;

public interface IUITranslationAuditLogRepository
{
    Task<PagedResult<TranslationAuditRowDto>> GetAuditLogsAsync(
        FilteredTranslationAuditRequestDto request,
        PagedRequest paging);
}
