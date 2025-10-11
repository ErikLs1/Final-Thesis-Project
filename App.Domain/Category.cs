using App.Domain.Base;

namespace App.Domain;

public class Category : IDomainId
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public ICollection<Category> Categories { get; set; }= new List<Category>();
}