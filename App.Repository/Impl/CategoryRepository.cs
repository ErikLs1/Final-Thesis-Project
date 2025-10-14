using App.Domain;
using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class CategoryRepository : ICategoryRepository
{
    public CategoryRepository(AppDbContext repositoryDbContext)
    {
    }

    public Task AddCategory(Category entity)
    {
        throw new NotImplementedException();
    }
}