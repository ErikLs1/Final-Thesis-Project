using App.Domain;
using App.Service.Dto;

namespace App.Service.Interface;

public interface ICategoryService
{
    Task<Category> CreateAsync(CategoryCreateDto dto);
}