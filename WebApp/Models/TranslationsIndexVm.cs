namespace WebApp.Models;

public class TranslationsIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public int? SelectedVersion { get; set; }

    public List<LanguageOption> Languages { get; set; } = new();
    public List<Row> Rows { get; set; } = new();

    public sealed class LanguageOption
    {
        public Guid Id { get; set; }
        public string Display { get; set; } = default!; 
    }

    public sealed class Row
    {
        public Guid ResourceKeyId { get; set; }
        public string ResourceKey { get; set; } = default!;
        public string FriendlyKey { get; set; } = default!;
        public string? Content { get; set; }
        public int? VersionNumber { get; set; }
        public string LanguageTag { get; set; } = default!;
    }
}