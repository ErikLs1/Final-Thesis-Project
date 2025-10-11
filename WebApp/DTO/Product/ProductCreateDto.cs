namespace WebApp.DTO.Product;

public class ProductCreateDto
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Quantity { get; set; }
}