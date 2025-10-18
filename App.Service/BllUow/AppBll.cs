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
}