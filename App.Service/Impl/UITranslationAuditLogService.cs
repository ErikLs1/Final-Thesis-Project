using App.Repository.DalUow;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
using App.Service.Interface;

namespace App.Service.Impl;

public class UITranslationAuditLogService : IUITranslationAuditLogService
{
    private readonly IAppUow _uow;
    
    public UITranslationAuditLogService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public Task<PagedResult<TranslationAuditRowDto>> GetAuditLogsAsync(
        FilteredTranslationAuditRequestDto request,
        PagedRequest paging)
    {
        return _uow.UITranslationAuditLogRepository.GetAuditLogsAsync(request, paging);
    }
}
