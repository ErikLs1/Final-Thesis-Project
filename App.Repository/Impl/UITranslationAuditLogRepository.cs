using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationAuditLogRepository: IUITranslationAuditLogRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationAuditLogRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}