using System.ComponentModel.DataAnnotations;

namespace App.Service.IdentityDto;

public class RegisterDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(128)]
    public string FirstName { get; set; } = default!;
    
    [Required]
    [MinLength(1)]
    [MaxLength(128)]
    public string LastName { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string Email { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string Address { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string PhoneNumber { get; set; } = default!;
    
    [Required]
    [MaxLength(128)]
    public string Gender { get; set; } = default!;
    
    [Required]
    public DateOnly DateOfBirth { get; set; } = default!;
}