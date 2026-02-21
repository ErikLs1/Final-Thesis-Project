using App.Repository.Pager;

namespace App.UnitTests.Pagination;

public class PagedRequestTests
{
    [Fact]
    public void Normalize_WhenPageAndPageSizeAreInvalid_UsesDefaults()
    {
        var request = new PagedRequest { Page = 0, PageSize = -5 };

        var normalized = request.Normalize();

        Assert.Equal(1, normalized.Page);
        Assert.Equal(10, normalized.PageSize);
        Assert.Equal(0, normalized.Skip);
    }

    [Fact]
    public void Normalize_WhenPageSizeIsNotAllowed_UsesDefaultPageSize()
    {
        var request = new PagedRequest { Page = 3, PageSize = 30 };

        var normalized = request.Normalize();

        Assert.Equal(3, normalized.Page);
        Assert.Equal(10, normalized.PageSize);
        Assert.Equal(20, normalized.Skip);
    }
}
