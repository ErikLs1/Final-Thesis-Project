using App.Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace App.Repository.DalUow;

public class BaseRepository<TDomainEntity> : BaseRepository<TDomainEntity, Guid>, IBaseRepository<TDomainEntity>
    where TDomainEntity : class, IDomainId
{
    public BaseRepository(DbContext repositoryDbContext) : base(repositoryDbContext)
    {
    }
}

public class BaseRepository<TDomainEntity, TKey> : IBaseRepository<TDomainEntity, TKey>
    where TDomainEntity : class, IDomainId<TKey>
    where TKey : IEquatable<TKey>
{
    protected readonly DbContext RepositoryDbContext;
    protected readonly DbSet<TDomainEntity> RepositoryDbSet;

    public BaseRepository(DbContext repositoryDbContext)
    {
        RepositoryDbContext = repositoryDbContext;
        RepositoryDbSet = RepositoryDbContext.Set<TDomainEntity>();
    }

    protected virtual IQueryable<TDomainEntity> GetQuery(TKey? userId = default!)
    {
        var query = RepositoryDbSet.AsQueryable();

        if (typeof(IDomainUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)) &&
            userId != null &&
            !EqualityComparer<TKey>.Default.Equals(userId, default))
        {
            query = query.Where(e => ((IDomainUserId<TKey>)e).UserId.Equals(userId));
        }

        return query;
    }

    public IEnumerable<TDomainEntity> All(TKey? userId = default)
    {
        return GetQuery(userId)
            .ToList()
            .Select(e => e!);
    }

    public async Task<IEnumerable<TDomainEntity>> AllAsync(TKey? userId = default)
    {
        return (await GetQuery(userId)
                .ToListAsync())
            .Select(e => e!);
    }

    public TDomainEntity? Find(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        var res = query.FirstOrDefault(e => e.Id.Equals(id));
        return res;
    }

    public async Task<TDomainEntity?> FindAsync(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        var res = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        return res;
    }

    public void Add(TDomainEntity entity, TKey? userId = default)
    {
        if (typeof(IDomainUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)) &&
            userId != null &&
            !EqualityComparer<TKey>.Default.Equals(userId, default))
        {
            ((IDomainUserId<TKey>) entity!).UserId = userId;
        }
        
        RepositoryDbSet.Add(entity!);
    }

    public TDomainEntity? Update(TDomainEntity entity, TKey? userId = default)
    {
        if (ShouldUseUserId(userId))
        {
            var dbEntity = RepositoryDbSet
                .AsNoTracking()
                .FirstOrDefault(e => e.Id.Equals(entity.Id));

            if (dbEntity == null || !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
        }

        return RepositoryDbSet.Update(entity!).Entity!;
    }

    public async Task<TDomainEntity?> UpdateAsync(TDomainEntity entity, TKey? userId = default)
    {
        if (ShouldUseUserId(userId))
        {
            var dbEntity = await RepositoryDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id.Equals(entity.Id));

            if (dbEntity == null || !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
            if (ShouldUseUserId(userId) && !((IDomainUserId<TKey>)dbEntity).UserId.Equals(userId)) return null;
        }
        
        return RepositoryDbSet.Update(domainEntity).Entity!;
    }

    public void Remove(TDomainEntity entity, TKey? userId = default)
    {
        Remove(entity.Id, userId);
    }

    public void Remove(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        query = query.Where(e => e.Id.Equals(id));
        var dbEntity = query.FirstOrDefault();
        if (dbEntity != null)
        {
            RepositoryDbSet.Remove(dbEntity);
        }
    }

    public async Task RemoveAsync(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        query = query.Where(e => e.Id.Equals(id));
        var dbEntity = await query.FirstOrDefaultAsync();
        if (dbEntity != null)
        {
            RepositoryDbSet.Remove(dbEntity);
        }
    }

    public bool Exists(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        return query.Any(e => e.Id.Equals(id));
    }

    public async Task<bool> ExistsAsync(TKey id, TKey? userId = default)
    {
        var query = GetQuery(userId);
        return await query.AnyAsync(e => e.Id.Equals(id));
    }
    
    private bool ShouldUseUserId(TKey? userId = default!)
    {
        return typeof(IDomainUserId<TKey>).IsAssignableFrom(typeof(TDomainEntity)) &&
               userId != null &&
               !EqualityComparer<TKey>.Default.Equals(userId, default);
    }
}