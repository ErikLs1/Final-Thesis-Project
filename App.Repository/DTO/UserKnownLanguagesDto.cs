namespace App.Repository.DTO;

public class UserKnownLanguagesDto
{
    public Guid Id { get; set; }
    public string LanguageName { get; set; } = default!;
    public string LanguageTag { get; set; } = default!;
    public string DisplayValue => $"{LanguageName} ({LanguageTag})";
}