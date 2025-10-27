using App.Domain.Enum;
using WebApp.Models.Shared;

namespace WebApp.Models.Admin.Translations;

public class AdminTranslationsIndexVm
{
    public Guid? SelectedLanguageId { get; set; }
    public int? SelectedVersion { get; set; }
    public TranslationState? SelectedState { get; set; }
    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<AdminTranslationsIndexRowVm> Rows { get; set; } = new();
}