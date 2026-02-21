using App.Repository.Pager;

namespace App.UnitTests.Pagination;

public class PagedResultTests
{
    [Fact]
    public void PagingFlags_AreComputedCorrectly()
    {
        var result = new PagedResult<int>
        {
            Items = [1, 2],
            TotalCount = 21,
            Page = 2,
            PageSize = 10
        };

        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPrev);
        Assert.True(result.HasNext);
    }
}
