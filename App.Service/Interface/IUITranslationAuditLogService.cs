using App.Repository.DTO.UITranslations;
using App.Repository.Pager;

namespace App.Service.Interface;

public interface IUITranslationAuditLogService
{
    Task<PagedResult<TranslationAuditRowDto>> GetAuditLogsAsync(
        FilteredTranslationAuditRequestDto request,
        PagedRequest paging);
}
