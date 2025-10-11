using WebApp.DTO.Product;

namespace WebApp.Mappers.Product;

public class ProductMapper
{
    public static App.Domain.Product Map(ProductCreateDto dto)
    {
        return new App.Domain.Product
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity
        };
    }
}