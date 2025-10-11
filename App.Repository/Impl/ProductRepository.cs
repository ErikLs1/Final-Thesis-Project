using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class ProductRepository : IProductRepository
{
    public ProductRepository(AppDbContext repositoryDbContext)
    {
    }
}