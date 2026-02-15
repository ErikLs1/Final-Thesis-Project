namespace WebApp.Helpers.Translations.Interfaces;

public interface ITranslationCache
{
    Task<IReadOnlyDictionary<string, string>> GetLanguageMapAsync(string languageTag);
    Task InvalidateAsync(string languageTag);
}