using App.Domain;

namespace App.Repository.Interface;

public interface ICategoryRepository
{
    Task AddCategory(Category entity);
}