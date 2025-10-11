using App.Repository.DalDto;

namespace App.Repository.Interface;

public interface IRefreshTokenRepository
{
    Task<int> DeleteExpiredTokenAsync(Guid userId);
    
    Task<IList<RefreshTokenDalDto>> FindByTokenAsync(Guid userId, string token);

    Task<int> DeleteByTokenAsync(Guid userId, string token);
}