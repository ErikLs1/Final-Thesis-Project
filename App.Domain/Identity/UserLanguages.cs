namespace App.Domain.Identity;

// TODO: IMPLEMENT LATER
public class UserLanguages
{
    public Guid Id { get; set; }
    public Guid LanguageId { get; set; }
    public Guid UserId { get; set; }

    public Languages Language { get; set; } = null!;
    public User User { get; set; } = null!;
}