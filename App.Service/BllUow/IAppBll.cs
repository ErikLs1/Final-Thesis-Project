using App.Service.Interface;

namespace App.Service.BllUow;

public interface IAppBll : IBaseBll
{
    IUserLanguageService UserLanguageService { get; }
    ILanguageService LanguageService { get; }
    IUIResourceKeysService UIResourceKeysService { get; }
    IUITranslationAuditLogService UITranslationAuditLogService { get; }
    IUITranslationService UITranslationService { get; }
    IUITranslationsVersionsService UITranslationsVersionsService { get; }
}