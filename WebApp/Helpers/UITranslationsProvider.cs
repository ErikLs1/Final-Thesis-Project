using System.Globalization;
using App.Service.BllUow;
using Microsoft.AspNetCore.Localization;

namespace WebApp.Helpers;

public class UITranslationsProvider : IUITranslationsProvider 
{
    private readonly IAppBll _bll;
    private readonly IHttpContextAccessor _http;

    private Dictionary<string, string>? _map;
    public string LanguageTag { get; }

    public UITranslationsProvider(IAppBll bll, IHttpContextAccessor http)
    {
        _bll = bll;
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
        
        _map = _bll.UITranslationService
            .GetLiveTranslationsByLanguageTagAsync(LanguageTag)
            .GetAwaiter().GetResult();
    }
}