using WebApp.Redis.Services;

namespace WebApp.Extensions.Builder;

public static class TranslationsCacheBuilderExtension
{
    public static async Task WarmupTranslationsCacheAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var redisTranslations = scope.ServiceProvider.GetRequiredService<IRedisTranslationService>();
            await redisTranslations.WarmupAsync();
        }
        catch (Exception e)
        {
            app.Logger.LogWarning(e, "UI TRANSLATIONS CACHE WARMUP FAILED");
        }
    }
}