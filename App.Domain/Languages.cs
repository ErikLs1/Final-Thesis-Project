using App.Domain.Base;
using App.Domain.Identity;

namespace App.Domain;

// TODO: IMPLEMENT LATER
public class Languages : IDomainId
{
    public Guid Id { get; set; }

    public ICollection<UserLanguages> UserLanguages { get; set; }= new List<UserLanguages>();
}