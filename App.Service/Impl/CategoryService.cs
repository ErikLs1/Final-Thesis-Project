using App.Domain;
using App.Repository.DalUow;
using App.Service.Dto;
using App.Service.Interface;

namespace App.Service.Impl;

public class CategoryService : ICategoryService
{
    private readonly IAppUow _uow;
    
    public CategoryService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public async Task<Category> CreateAsync(CategoryCreateDto dto)
    {
        var entity = new Category
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _uow.CategoryRepository.AddCategory(entity);
        await _uow.SaveChangesAsync();

        return entity;
    }
}