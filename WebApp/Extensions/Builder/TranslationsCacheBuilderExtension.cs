using App.Service.BllUow;
using WebApp.Helpers.Translations.Interfaces;

namespace WebApp.Extensions.Builder;

public static class TranslationsCacheBuilderExtension
{
    public static async Task WarmupTranslationsCacheAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            // warm the cache
            var cache = scope.ServiceProvider.GetRequiredService<ITranslationCache>();
            var bll = scope.ServiceProvider.GetRequiredService<IAppBll>();
            
            var allLanguages = await bll.LanguageService.GetAllLanguages();
            foreach (var lang in allLanguages)
            {
                if (string.IsNullOrWhiteSpace(lang.Tag)) continue;
                await cache.GetLanguageMapAsync(lang.Tag);
            }
        }
        catch (Exception e)
        {
            app.Logger.LogWarning(e, "UI TRANSLATIONS CACHE WARMUP FAILED");
        }
    }
}