namespace WebApp.Models.Shared;

public class LanguageOptionVm
{
    public Guid Id { get; set; }
    public string Display { get; set; } = default!;
    public string LanguageName { get; set; } = default!;
    public string LanguageTag { get; set; } = default!;
}