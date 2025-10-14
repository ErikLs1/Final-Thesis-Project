using App.Service.Dto;

namespace WebApp.Mappers.Category;

public class CategoryMapper
{
    public static App.Domain.Category Map(CategoryCreateDto dto)
    {
        return new App.Domain.Category()
        {
            Name = dto.Name,
            Description = dto.Description
        };
    }
}