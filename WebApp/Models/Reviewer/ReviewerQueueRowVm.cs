using App.Domain.Enum;

namespace WebApp.Models.Reviewer;

public class ReviewerQueueRowVm
{
    public Guid TranslationVersionId { get; set; }
    public string LanguageTag { get; set; } = default!;
    public string ResourceKey { get; set; } = default!;
    public string FriendlyKey { get; set; } = default!;
    public int VersionNumber { get; set; }
    public string Content { get; set; } = default!;
    public TranslationState TranslationState { get; set; }
}
