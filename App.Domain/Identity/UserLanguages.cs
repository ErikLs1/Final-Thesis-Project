using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class UserLanguages
{
    public Guid Id { get; set; }
    public Guid LanguageId { get; set; }
    public Guid UserId { get; set; }

    public Languages Language { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}