using WebApp.Models.Shared;

namespace WebApp.Models.Reviewer;

public class ReviewerQueueVm
{
    public Guid SelectedLanguageId { get; set; }
    public int? SelectedVersion { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<ReviewerQueueRowVm> Rows { get; set; } = new();
}
