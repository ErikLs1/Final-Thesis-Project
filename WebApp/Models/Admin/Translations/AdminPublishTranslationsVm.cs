namespace WebApp.Models.Admin.Translations;

public class AdminPublishTranslationsVm
{
    public Guid SelectedLanguageId { get; set; }
    public string SelectedLanguageTag { get; set; } = default!; 
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    public List<AdminPublishTranslationVersionVm> Rows { get; set; } = new();
}