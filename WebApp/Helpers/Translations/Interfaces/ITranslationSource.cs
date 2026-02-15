namespace WebApp.Helpers.Translations.Interfaces;

public interface ITranslationSource
{
    Task<Dictionary<string, string>> LoadAsync(string languageTag);
}