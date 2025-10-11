using App.Domain.Identity;
using App.Repository.DalDto;
using App.Repository.DalUow;
using App.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.Impl;

public class RefreshTokenRepository : BaseRepository<RefreshToken> ,IRefreshTokenRepository
{
    public RefreshTokenRepository(DbContext repositoryDbContext) : base(repositoryDbContext)
    {
    }
    
    public async Task<int> DeleteExpiredTokenAsync(Guid userId)
    {
        return await GetQuery(userId)
            .Where(t => t.UserId == userId && t.Expiration < DateTime.UtcNow)
            .ExecuteDeleteAsync();
    }

    public async Task<IList<RefreshTokenDalDto>> FindByTokenAsync(Guid userId, string token)
    {
        return await GetQuery(userId)
            .AsNoTracking()
            .Where(x => 
                (x.RefreshToken == token && x.Expiration > DateTime.UtcNow) ||
                (x.PreviousRefreshToken == token && x.PreviousExpiration > DateTime.UtcNow)
            )
            .Select(t => Mapper.Map(t)!)
            .ToListAsync();
    }

    public async Task<int> DeleteByTokenAsync(Guid userId, string token)
    {
        return await GetQuery(userId)
            .Where(x =>
                (x.RefreshToken == token) ||
                (x.PreviousRefreshToken == token))
            .ExecuteDeleteAsync();
    }
}