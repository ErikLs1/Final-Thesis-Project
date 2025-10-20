using App.Domain.Enum;

namespace WebApp.Models;

public class AdminTranslationsIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public List<Row> Rows { get; set; } = new();

    public sealed class Row
    {
        public Guid TranslationVersionId { get; set; }
        public string LanguageTag { get; set; } = default!;
        public string ResourceKey { get; set; } = default!;
        public string FriendlyKey { get; set; } = default!;
        public int VersionNumber { get; set; }
        public string Content { get; set; } = default!;
        public TranslationState TranslationState { get; set; }
        public DateTime PublishedAt { get; set; }
        public string PublishedBy { get; set; } = default!;
    }
}