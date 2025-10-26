namespace WebApp.Models.Translator.Languages;

public class TranslatorKnownLanguageRowVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Tag { get; set; } = default!;
}