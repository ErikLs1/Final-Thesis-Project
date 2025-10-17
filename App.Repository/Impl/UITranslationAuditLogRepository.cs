using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationAuditLogRepository: IUITranslationAuditLogRepository
{
    public UITranslationAuditLogRepository(AppDbContext repositoryDbContext)
    {
    }
}