using System.Globalization;
using App.Service.Impl.Assemblies.Resx;
using Microsoft.AspNetCore.Localization;
using WebApp.Redis.Services;

namespace WebApp.Helpers;

public class UITranslationsProvider : IUITranslationsProvider 
{
    private readonly IRedisTranslationService _redisTranslation;
    private readonly IHttpContextAccessor _http;

    private Dictionary<string, string>? _map;
    public string LanguageTag { get; }

    public UITranslationsProvider(IRedisTranslationService redisTranslation, IHttpContextAccessor http)
    {
        _redisTranslation = redisTranslation;
        _http = http;

        LanguageTag =
            _http.HttpContext?.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture.Name
            ?? CultureInfo.CurrentUICulture.Name
            ?? "en";
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        EnsureLoaded();
        
        // First layer: Redis
        if (_map!.TryGetValue(key, out var v))
        {
            Console.WriteLine("REDIS" + v);
            return v;
        }
        
        // Second layer: DB
        // TODO: LATER
        
        // Third layer: Resx
        var culture = new CultureInfo(LanguageTag);
        foreach (var resourceManager in ResourceManagerRegistry.All)
        {
            var value = resourceManager.GetString(key, culture);
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine("RESX" + v);
                return value;
            }
        }
        
        return key;
    }

    private void EnsureLoaded()
    {
        if (_map != null) return;
        
        _map = _redisTranslation
            .GetTranslationsAsync(LanguageTag)
            .GetAwaiter()
            .GetResult();
    }
}