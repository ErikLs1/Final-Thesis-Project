namespace WebApp.Models.Translator.Versions;

public class TranslatorCreateNewVersionItemVm
{
    public Guid ResourceKeyId { get; set; }
    public string ResourceKey { get; set; } = default!;
    public string FriendlyKey { get; set; } = default!;
    public string? DefaultContent { get; set; }
    public bool Include { get; set; }
    public string? Content { get; set; }
}