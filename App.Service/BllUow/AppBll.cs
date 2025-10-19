using App.Repository.DalUow;
using App.Service.Impl;
using App.Service.Interface;

namespace App.Service.BllUow;

public class AppBll : BaseBll<IAppUow>, IAppBll
{
    public AppBll(IAppUow uow) : base(uow)
    {
    }

    public IProductService? _productService;

    public IProductService ProductService =>
        _productService ??= new ProductService(BllUow);
    
    public ICategoryService? _categoryService;

    public ICategoryService CategoryService =>
        _categoryService ??= new CategoryService(BllUow);
    
    public IUserLanguageService? _userLanguageService;
    public IUserLanguageService UserLanguageService =>
        _userLanguageService ??= new UserLanguageService(BllUow);
    
    public ILanguageService? _languageService;
    public ILanguageService LanguageService =>
        _languageService ??= new LanguageService(BllUow);
    
    public IUIExperimentService? _UIExperimentService;
    public IUIExperimentService UIExperimentService =>
        _UIExperimentService ??= new UIExperimentService(BllUow);
    
    public IUIResourceKeysService? _UIResourceKeysService;
    public IUIResourceKeysService UIResourceKeysService =>
        _UIResourceKeysService ??= new UIResourceKeysService(BllUow);
    
    public IUITranslationAuditLogService? _UITranslationAuditLogService;
    public IUITranslationAuditLogService UITranslationAuditLogService =>
        _UITranslationAuditLogService ??= new UITranslationAuditLogService(BllUow);
    
    public IUITranslationService? _UITranslationService;
    public IUITranslationService UITranslationService =>
        _UITranslationService ??= new UITranslationService(BllUow);
    
    public IUITranslationsVersionsService? _UITranslationsVersionsService;
    public IUITranslationsVersionsService UITranslationsVersionsService =>
        _UITranslationsVersionsService ??= new UITranslationsVersionsService(BllUow);
}