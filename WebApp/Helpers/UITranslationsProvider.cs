using System.Globalization;
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
        return _map!.TryGetValue(key, out var v) ? v : key;
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