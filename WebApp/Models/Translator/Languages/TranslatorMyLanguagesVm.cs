namespace WebApp.Models.Translator.Languages;

public class TranslatorMyLanguagesVm
{
    public List<TranslatorKnownLanguageRowVm> Selected { get; set; } = new();
    public bool HasAny => Selected.Count > 0;
}