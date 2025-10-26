using App.Service.BllUow;
using StackExchange.Redis;
using WebApp.Redis.Client;

namespace WebApp.Redis.Services.Impl;

public class RedisTranslationService : IRedisTranslationService
{
    private readonly IRedisClient _redis;
    private readonly IAppBll _bll;

    public RedisTranslationService(IRedisClient redis, IAppBll bll)
    {
        _redis = redis;
        _bll = bll;
    }
    
    public async Task<Dictionary<string, string>> GetTranslationsAsync(string languageTag)
    {
        var db = _redis.GetDb();
        var redisHashKey = $"ui:translations:{languageTag}";
        
        // Get db ui translations
        var dbTranslations = await _bll.UITranslationService.GetLiveTranslationsByLanguageTagAsync(languageTag);
        
        // Save translation to redis (Key -> Translation_key, Value -> Translation_string)
        var redisTranslations = dbTranslations
            .Select(x => new HashEntry(x.Key, x.Value))
            .ToArray();

        if (redisTranslations.Length > 0)
        {
            await db.HashSetAsync(redisHashKey, redisTranslations);
        }

        // Fallback 
        return dbTranslations;
    }
}