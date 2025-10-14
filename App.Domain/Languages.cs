using App.Domain.Identity;

namespace App.Domain;

public class Languages
{
    public Guid Id { get; set; }

    public ICollection<UserLanguages> UserLanguages { get; set; }= new List<UserLanguages>();
}