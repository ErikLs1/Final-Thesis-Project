namespace App.Service;

public interface IAppBll : IBaseBll
{
    IProductService ProductService { get; }
}