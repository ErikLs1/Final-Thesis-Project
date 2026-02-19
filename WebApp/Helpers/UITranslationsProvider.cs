using System.Globalization;
using App.Service.Impl.Assemblies.Resx;
using WebApp.Helpers.Translations.Interfaces;

namespace WebApp.Helpers;

public class UITranslationsProvider : IUITranslationsProvider 
{
    private readonly ITranslationCache _cache;
    private readonly CultureInfo _culture;
    
    private Task<IReadOnlyDictionary<string, string>>? _loadTask;
    private IReadOnlyDictionary<string, string>? _map;
    
    public string LanguageTag { get; }

    public UITranslationsProvider(ITranslationCache cache, string languageTag)
    {
        _cache = cache;
        LanguageTag = languageTag;
        _culture = CultureInfo.GetCultureInfo(languageTag);
    }

    private Task<IReadOnlyDictionary<string, string>> EnsureLoadedAsync()
    {
        // if loaded return
        if (_map != null) return Task.FromResult(_map);

        _loadTask ??= LoadAsync();
        return _loadTask;

        async Task<IReadOnlyDictionary<string, string>> LoadAsync()
        {
            var map = await _cache.GetLanguageMapAsync(LanguageTag);
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