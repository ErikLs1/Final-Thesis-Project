using App.Domain.Enum;
using WebApp.Models.Shared;

namespace WebApp.Models.Admin.Audit;

public class AdminTranslationAuditIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public TranslationAuditAction? SelectedActionType { get; set; }
    public string? ChangedBy { get; set; }
    public string? ResourceKeySearch { get; set; }
    public DateTime? ChangedFromUtc { get; set; }
    public DateTime? ChangedToUtc { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<AdminTranslationAuditIndexRowVm> Rows { get; set; } = new();
}
