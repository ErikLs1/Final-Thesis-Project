using Microsoft.EntityFrameworkCore;

namespace App.Repository.DalUow;

public class BaseUow<TDbContext> : IBaseUow
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext;
    
    public BaseUow(TDbContext context)
    {
        UowDbContext = context;
    }
    
    public async Task<int> SaveChangesAsync()
    {
        return await UowDbContext.SaveChangesAsync();
    }
}