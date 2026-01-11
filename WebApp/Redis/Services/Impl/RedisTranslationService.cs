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
        
        // 1 - Try to get values from Redis
        var cachedTranslations = await db.HashGetAllAsync(redisHashKey);
        if (cachedTranslations.Length > 0)
        {
            return cachedTranslations.ToDictionary(
                x => x.Name.ToString(),
                x => x.Value.ToString()
            );
        }
        
        // 2 - If no values in redis take them from db
        var dbTranslations = await _bll.UITranslationService
            .GetLiveTranslationsByLanguageTagAsync(languageTag);
        
        // 3 - Put found translations to redis
        if (dbTranslations.Count > 0)
        {
            var entries = dbTranslations.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
            await db.HashSetAsync(redisHashKey, entries);
            await db.KeyExpireAsync(redisHashKey, TimeSpan.FromMinutes(30));
        }

        return dbTranslations;
    }
}