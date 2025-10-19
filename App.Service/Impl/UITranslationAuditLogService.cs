using App.Repository.DalUow;
using App.Service.Interface;

namespace App.Service.Impl;

public class UITranslationAuditLogService : IUITranslationAuditLogService
{
    private readonly IAppUow _uow;
    
    public UITranslationAuditLogService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }
}