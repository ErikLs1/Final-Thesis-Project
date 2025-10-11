namespace App.Service.IdentityDto;

public class JwtResponseDto
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public string Role { get; set; } = default!;
}