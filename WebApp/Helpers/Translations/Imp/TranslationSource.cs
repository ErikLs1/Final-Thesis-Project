using App.Service.BllUow;
using WebApp.Helpers.Translations.Interfaces;

namespace WebApp.Helpers.Translations.Imp;

public sealed class TranslationSource : ITranslationSource
{
    private readonly IAppBll _bll;

    public TranslationSource(IAppBll bll)
    {
        _bll = bll;
    }
    
    public Task<Dictionary<string, string>> LoadAsync(string languageTag)
    {
        return _bll.UITranslationService.GetLiveTranslationsByLanguageTagAsync(languageTag);
    }
}