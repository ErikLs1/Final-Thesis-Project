namespace WebApp.Redis.Services;

public interface IRedisTranslationService
{
    Task<Dictionary<string, string>> GetTranslationsAsync(string languageTag);
    Task InvalidateTranslationsAsync(string languageTag);
    Task<Dictionary<string, string>> RefreshTranslationAsync(string languageTag);
}