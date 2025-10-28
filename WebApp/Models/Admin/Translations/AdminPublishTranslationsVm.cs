namespace WebApp.Models.Admin.Translations;

public class AdminPublishTranslationsVm
{
    public Guid SelectedLanguageId { get; set; }
    public string SelectedLanguageTag { get; set; } = default!; 
    public List<AdminPublishTranslationVersionVm> Rows { get; set; } = new();
}