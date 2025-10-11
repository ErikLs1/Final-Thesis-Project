using App.EF;
using App.Repository.Impl;
using App.Repository.Interface;

namespace App.Repository;

public class AppUow : BaseUow<AppDbContext>, IAppUow
{
    public AppUow(AppDbContext uowDbContext) : base(uowDbContext)
    {
    }

    public IProductRepository? _productRepository;

    public IProductRepository ProductRepository =>
        _productRepository ??= new ProductRepository(UowDbContext);
}