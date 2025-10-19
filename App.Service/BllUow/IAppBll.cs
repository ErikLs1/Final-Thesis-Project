using App.Service.Interface;

namespace App.Service.BllUow;

public interface IAppBll : IBaseBll
{
    IProductService ProductService { get; }
    ICategoryService CategoryService { get; }
    IUserLanguageService UserLanguageService { get; }
    ILanguageService LanguageService { get; }
    IUIExperimentService UIExperimentService { get; }
    IUIResourceKeysService UIResourceKeysService { get; }
    IUITranslationAuditLogService UITranslationAuditLogService { get; }
    IUITranslationService UITranslationService { get; }
    IUITranslationsVersionsService UITranslationsVersionsService { get; }
}