namespace WebApp.Models.Admin.Translations;

public class AdminPublishTranslationVersionVm
{
    public Guid TranslationVersionId { get; set; }
    public string FriendlyKey { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int VersionNumber { get; set; }
    public string LanguageTag { get; set; } = default!;
    public string CurrentState { get; set; } = default!;
    public bool Selected { get; set; }
}