using App.Repository;

namespace App.Service.Impl;

public class ProductService : IProductService
{
    public ProductService(IAppUow serviceUow)
    {
    }
}