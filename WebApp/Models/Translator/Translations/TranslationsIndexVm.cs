using WebApp.Models.Shared;

namespace WebApp.Models.Translator.Translations;

public class TranslationsIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public int? SelectedVersion { get; set; }

    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<TranslatorTranslationsRowVm> Rows { get; set; } = new();
}