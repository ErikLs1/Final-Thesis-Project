using App.Domain;

namespace App.Repository.Interface;

public interface IProductRepository
{
    Task AddProduct(Product entity);
}