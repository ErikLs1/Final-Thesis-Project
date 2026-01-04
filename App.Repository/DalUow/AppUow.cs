using App.EF;
using App.Repository.Impl;
using App.Repository.Impl.ResxImport;
using App.Repository.Interface;
using App.Repository.Interface.ResxImport;
using Microsoft.Extensions.DependencyInjection;

namespace App.Repository.DalUow;

public class AppUow : BaseUow<AppDbContext>, IAppUow
{
    private readonly IServiceProvider _sp;
    
    public AppUow(AppDbContext uowDbContext, IServiceProvider sp) : base(uowDbContext)
    {
        _sp = sp;
    }

    public IProductRepository? _productRepository;
    public IProductRepository ProductRepository =>
        _productRepository ??= new ProductRepository(UowDbContext);
    
    public ICategoryRepository? _categoryRepository;
    public ICategoryRepository CategoryRepository =>
        _categoryRepository ??= new CategoryRepository(UowDbContext);

    public ILanguageRepository? _languageRepository;
    public ILanguageRepository LanguageRepository =>
        _languageRepository ??= new LanguageRepository(UowDbContext);
    
    public IResxImportRepository? _resxImportRepository;

    public IResxImportRepository ResxImportRepository =>
        _resxImportRepository ??= ActivatorUtilities.CreateInstance<ResxImportRepository>(_sp, UowDbContext);
    
    public IUIExperimentRepository? _UIExperimentRepository;
    public IUIExperimentRepository UIExperimentRepository =>
        _UIExperimentRepository ??= new UIExperimentRepository(UowDbContext);
    
    public IUIResourceKeysRepository? _UIResourceKeysRepository;
    public IUIResourceKeysRepository UIResourceKeysRepository =>
        _UIResourceKeysRepository ??= new UIResourceKeysRepository(UowDbContext);
    
    public IUITranslationAuditLogRepository? _UITranslationAuditLogRepository;
    public IUITranslationAuditLogRepository UITranslationAuditLogRepository =>
        _UITranslationAuditLogRepository ??= new UITranslationAuditLogRepository(UowDbContext);
    
    public IUITranslationRepository? _UITranslationRepository;
    public IUITranslationRepository UITranslationRepository =>
        _UITranslationRepository ??= new UITranslationRepository(UowDbContext);
    
    public IUITranslationsVersionsRepository? _UITranslationsVersionsRepository;
    public IUITranslationsVersionsRepository UITranslationsVersionsRepository =>
        _UITranslationsVersionsRepository ??= new UITranslationsVersionsRepository(UowDbContext);
    
    public IUserLanguageRepository? _userLanguageRepository;
    public IUserLanguageRepository UserLanguageRepository =>
        _userLanguageRepository ??= new UserLanguageRepository(UowDbContext);
}