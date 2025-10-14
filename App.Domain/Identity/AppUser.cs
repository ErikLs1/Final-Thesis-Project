using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUser : IdentityUser<Guid>
{
    public ICollection<UserLanguages> UserLanguages { get; set; } = new List<UserLanguages>();
}