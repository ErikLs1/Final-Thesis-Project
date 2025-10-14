using App.Service.Interface;

namespace App.Service.BllUow;

public interface IAppBll : IBaseBll
{
    IProductService ProductService { get; }
    ICategoryService CategoryService { get; }
}