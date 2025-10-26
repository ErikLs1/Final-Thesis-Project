using WebApp.Models.Shared;

namespace WebApp.Models.Translator.Versions;

public class CreateVersionsVm
{
    public Guid LanguageId { get; set; }

    public List<LanguageOptionVm> LanguageOptions { get; set; } = new();
    public List<TranslatorCreateNewVersionItemVm> Items { get; set; } = new();
}