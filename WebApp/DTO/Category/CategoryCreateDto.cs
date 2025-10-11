namespace WebApp.DTO.Category;

public class CategoryCreateDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
}