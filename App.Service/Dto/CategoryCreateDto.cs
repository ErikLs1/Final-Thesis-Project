namespace App.Service.Dto;

public class CategoryCreateDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = string.Empty;
}