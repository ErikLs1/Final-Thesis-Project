using App.Domain.Enum;
using WebApp.Models.Shared;

namespace WebApp.Models.Translator.Translations;

public class TranslationsIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public int? SelectedVersion { get; set; }
    public TranslationState? SelectedState { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<TranslatorTranslationsRowVm> Rows { get; set; } = new();
}