using System.ComponentModel.DataAnnotations;

namespace App.Service.IdentityDto;

public class LogoutDto
{
    [MaxLength(128)]
    [Required]
    public string RefreshToken { get; set; } = default!;
}