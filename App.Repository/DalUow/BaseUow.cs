using Microsoft.EntityFrameworkCore;

namespace App.Repository;

public class BaseUow<TDbContext> : IBaseUow
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext;
    
    public BaseUow(TDbContext context)
    {
        UowDbContext = context;
    }
    
    public Task<int> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}