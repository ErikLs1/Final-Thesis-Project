namespace App.Service.IdentityDto;

public class RefreshTokenDto
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}