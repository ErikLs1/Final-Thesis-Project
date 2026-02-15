using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Helpers.Translations.Options;
using WebApp.Redis.Client;

namespace WebApp.Helpers.Translations.Imp;

public sealed class TranslationDistributedCache : ITranslationDistributedCache
{
    private readonly IRedisClient _redis;
    private readonly TranslationCacheOptions _options;

    public TranslationDistributedCache(IRedisClient redis, IOptions<TranslationCacheOptions> options)
    {
        _redis = redis;
        _options = options.Value;
    }

    private string Key(string languageTag) => $"{_options.RedisKeyPrefix}:{languageTag}";
    
    public async Task<Dictionary<string, string>?> TryGetAsync(string languageTag)
    {
        var db = _redis.GetDb();
        var key = Key(languageTag);

        var entries = await db.HashGetAllAsync(key);

        if (entries.Length == 0) return null;

        var map = new Dictionary<string, string>(entries.Length, StringComparer.Ordinal);

        foreach (var e in entries)
        {
            map[e.Name!] = e.Value!;
        }

        return map;
    }

    public async Task SetAsync(string languageTag, Dictionary<string, string> map, TimeSpan ttl)
    {
        var db = _redis.GetDb();
        var key = Key(languageTag);

        if (map.Count == 0)
        {
            await db.StringSetAsync(key, "", ttl);
            return;
        }

        var arr = map.Select(kv => new HashEntry(kv.Key, kv.Value)).ToArray();
        await db.HashSetAsync(key, arr);
        await db.KeyExpireAsync(key, ttl);
    }

    public Task RemoveAsync(string languageTag)
    {
        var db = _redis.GetDb();
        return db.KeyDeleteAsync(Key(languageTag));
    }
}