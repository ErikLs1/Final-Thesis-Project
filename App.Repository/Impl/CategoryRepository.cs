using App.Domain;
using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    
    public CategoryRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }

    public async Task AddCategory(Category entity)
    {
        await _db.Categories.AddAsync(entity);
    }
}