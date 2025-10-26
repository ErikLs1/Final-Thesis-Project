namespace WebApp.Redis.Services;

public interface IRedisTranslationService
{
    Task<Dictionary<string, string>> GetTranslationsAsync(string languageTag);
}