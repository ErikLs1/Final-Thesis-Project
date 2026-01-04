using App.Repository.Interface;
using App.Repository.Interface.ResxImport;

namespace App.Repository.DalUow;

public interface IAppUow : IBaseUow
{
    IProductRepository ProductRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    ILanguageRepository LanguageRepository { get; }
    IResxImportRepository ResxImportRepository { get; }
    IUIExperimentRepository UIExperimentRepository { get; }
    IUIResourceKeysRepository UIResourceKeysRepository { get; }
    IUITranslationAuditLogRepository UITranslationAuditLogRepository { get; }
    IUITranslationRepository UITranslationRepository { get; }
    IUITranslationsVersionsRepository UITranslationsVersionsRepository { get; }
    IUserLanguageRepository UserLanguageRepository { get; }
}