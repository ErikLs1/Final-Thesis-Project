using App.Domain.Enum;

namespace WebApp.Models.Admin.Audit;

public class AdminTranslationAuditIndexRowVm
{
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = default!;
    public TranslationAuditAction ActionType { get; set; }
    public string LanguageTag { get; set; } = default!;
    public string ResourceKey { get; set; } = default!;
    public string FriendlyKey { get; set; } = default!;
    public int VersionNumber { get; set; }
    public TranslationState? OldState { get; set; }
    public TranslationState? NewState { get; set; }
    public string? OldContent { get; set; }
    public string? NewContent { get; set; }
}
