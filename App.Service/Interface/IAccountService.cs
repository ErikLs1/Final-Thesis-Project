using App.Service.IdentityDto;

namespace App.Service.Interface;

public interface IAccountService
{
    Task<JwtResponseDto> RegisterAsync(
        RegisterDto dto,
        int? jwtExpiresInSeconds,
        int? refreshTokenExpiresInSeconds);
    
    Task<JwtResponseDto> LoginAsync(
        LoginDto dto,
        int? jwtExpiresInSeconds,
        int? refreshTokenExpiresInSeconds);
    
    Task<JwtResponseDto> RenewTokenAsync(
        RefreshTokenDto dto,
        int? jwtExpiresInSeconds,
        int? refreshTokenExpiresInSeconds);
    Task LogoutAsync(Guid userId, string refreshToken);
}