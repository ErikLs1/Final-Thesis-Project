using App.Domain.Common;

namespace App.Domain;

public class Product : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public int Quantity { get; set; }
}