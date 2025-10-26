namespace WebApp.Models.Translator.Translations;

public class TranslatorTranslationsRowVm
{
    public Guid ResourceKeyId { get; set; }
    public string ResourceKey { get; set; } = default!;
    public string FriendlyKey { get; set; } = default!;
    public string? Content { get; set; }
    public int? VersionNumber { get; set; }
    public string LanguageTag { get; set; } = default!;
}