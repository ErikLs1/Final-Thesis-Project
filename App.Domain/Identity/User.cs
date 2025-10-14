namespace App.Domain.Identity;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserLanguages> UserLanguages { get; set; } = new List<UserLanguages>();
}