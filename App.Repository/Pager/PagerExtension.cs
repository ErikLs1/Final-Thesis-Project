using Microsoft.EntityFrameworkCore;

namespace App.Repository.Pager;

public static class PagerExtension
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedRequest request)
    {
        request = request.Normalize();

        var total = await query.CountAsync();
        
        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}