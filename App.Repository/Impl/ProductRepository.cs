using App.Domain;
using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class ProductRepository : IProductRepository
{
    public ProductRepository(AppDbContext repositoryDbContext)
    {
    }

    public Task AddProduct(Product entity)
    {
        throw new NotImplementedException();
    }
}