using App.Domain;
using App.EF;
using App.Repository.DalUow;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext repositoryDbContext) : base(repositoryDbContext)
    {
    }

    public Task AddProduct(Product entity)
    {
        throw new NotImplementedException();
    }
}