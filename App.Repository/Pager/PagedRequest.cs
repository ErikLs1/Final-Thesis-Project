namespace App.Repository.Pager;

public class PagedRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int Skip => (Page - 1) * PageSize;

    public static readonly int[] AllowedPageSizes = [10, 25, 50, 100];
    public const int MaxPageSize = 200;

    public PagedRequest Normalize()
    {
        var page = Page < 1 ? 1 : Page;

        var size = PageSize;
        if (size < 1) size = 10;
        if (size > MaxPageSize) size = MaxPageSize;
        
        // dropdown size restriction
        if (!AllowedPageSizes.Contains(size)) size = 10;

        return new PagedRequest { Page = page, PageSize = size };
    }
}