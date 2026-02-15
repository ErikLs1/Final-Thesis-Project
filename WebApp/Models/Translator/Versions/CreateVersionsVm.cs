using WebApp.Models.Shared;

namespace WebApp.Models.Translator.Versions;

public class CreateVersionsVm
{
    public Guid LanguageId { get; set; }
    
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<TranslatorCreateNewVersionItemVm> Items { get; set; } = new();
}