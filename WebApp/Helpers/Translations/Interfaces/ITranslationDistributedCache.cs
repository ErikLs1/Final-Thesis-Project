namespace WebApp.Helpers.Translations.Interfaces;

public interface ITranslationDistributedCache
{
    Task<Dictionary<string, string>?> TryGetAsync(string languageTag);
    Task SetAsync(string languageTag, Dictionary<string, string> map, TimeSpan ttl);
    Task RemoveAsync(string languageTag);
}