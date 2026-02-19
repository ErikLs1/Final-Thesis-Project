using App.Repository.Interface;
using App.Repository.Interface.ResxImport;

namespace App.Repository.DalUow;

public interface IAppUow : IBaseUow
{
    ILanguageRepository LanguageRepository { get; }
    IResxImportRepository ResxImportRepository { get; }
    IUIResourceKeysRepository UIResourceKeysRepository { get; }
    IUITranslationAuditLogRepository UITranslationAuditLogRepository { get; }
    IUITranslationRepository UITranslationRepository { get; }
    IUITranslationsVersionsRepository UITranslationsVersionsRepository { get; }
    IUserLanguageRepository UserLanguageRepository { get; }
}