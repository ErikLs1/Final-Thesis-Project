using System.Globalization;
using App.Service.Impl.Assemblies.Resx;
using Microsoft.AspNetCore.Localization;
using WebApp.Redis.Services;

namespace WebApp.Helpers;

public class UITranslationsProvider : IUITranslationsProvider 
{
    private readonly IRedisTranslationService _redisTranslation;
    private readonly CultureInfo _culture;
    
    private Task<Dictionary<string, string>>? _loadTask;
    private Dictionary<string, string>? _map;
    
    public string LanguageTag { get; }

    public UITranslationsProvider(IRedisTranslationService redisTranslation, string languageTag)
    {
        _redisTranslation = redisTranslation;
        LanguageTag = languageTag;
        _culture = CultureInfo.GetCultureInfo(languageTag);
    }

    private Task<Dictionary<string, string>> EnsureLoadedAsync()
    {
        // if loaded return
        if (_map != null) return Task.FromResult(_map);
        
        _loadTask ??= LoadAsync();
        return _loadTask;
        
        async Task<Dictionary<string, string>> LoadAsync()
        {
            var map = await _redisTranslation.GetTranslationsAsync(LanguageTag);
            _map = map;
            return map;
        }
    }

    public async Task<string> GetAsync(string key)
    {
        var map = await EnsureLoadedAsync();
        
        // First layer: Redis
        if (map.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v))
            return v;
        
        // Third layer: Resx
        var culture = new CultureInfo(LanguageTag);
        foreach (var resourceManager in ResourceManagerRegistry.All)
        {
            var value = resourceManager.GetString(key, culture);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        
        return key;
    }
}