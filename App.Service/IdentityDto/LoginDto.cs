using System.ComponentModel.DataAnnotations;

namespace App.Service.IdentityDto;

public class LoginDto
{
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    
    [MaxLength(128)]
    public string Password { get; set; } = default!;
}