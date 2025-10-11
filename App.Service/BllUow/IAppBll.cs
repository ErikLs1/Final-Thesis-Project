using App.Service.Interface;

namespace App.Service;

public interface IAppBll : IBaseBll
{
    IProductService ProductService { get; }
}