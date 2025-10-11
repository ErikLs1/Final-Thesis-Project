using App.Domain.Base;
using App.Domain.Base.Identity;

namespace App.Domain.Identity;

public class User : BaseUser<UserRole>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    
    [MinLength(1)]
    [MaxLength(128)]
    public string LastName { get; set; } = default!;

    public ICollection<UserLanguages> UserLanguages { get; set; } = new List<UserLanguages>();
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
}