using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WebApp.Helpers.Translations.Interfaces;
using WebApp.Helpers.Translations.Options;

namespace WebApp.Helpers.Translations.Imp;

public class TranslationCache : ITranslationCache
{
    private readonly IMemoryCache _memory;
    private readonly ITranslationDistributedCache _distributed;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TranslationCacheOptions _options;
    
    // single flight
    // https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items
    private readonly ConcurrentDictionary<string, Lazy<Task<Dictionary<string, string>>>> _flight =
        new(StringComparer.OrdinalIgnoreCase);

    public TranslationCache(
        IMemoryCache memory, 
        ITranslationDistributedCache distributed,
        IServiceScopeFactory scopeFactory,
        IOptions<TranslationCacheOptions> options)
    {
        _memory = memory;
        _distributed = distributed;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    private string MemoryKey(string languageTag) => $"translations:map:{languageTag}";
    
    public async Task<IReadOnlyDictionary<string, string>> GetLanguageMapAsync(string languageTag)
    {
        var memKey = MemoryKey(languageTag);

        // L1 - In-memory cache
        if (_memory.TryGetValue(memKey, out IReadOnlyDictionary<string, string>? cached) && cached != null)
            return cached;
        
        // L2 - Distributed cache
        var redisCache = await _distributed.TryGetAsync(languageTag);
        if (redisCache != null)
        {
            SetMemory(memKey, redisCache);
            return redisCache;
        }
        
        // Cache miss - singe flight
        var lazy = _flight.GetOrAdd(languageTag, _ => 
            new Lazy<Task<Dictionary<string, string>>>(() => LoadPopulateAsync(languageTag), isThreadSafe: true));

        try
        {
            var loaded = await lazy.Value;
            SetMemory(memKey, loaded);
            return loaded;
        }
        finally
        {
            _flight.TryRemove(languageTag, out _);
        }
    }

    public async Task InvalidateAsync(string languageTag)
    {
        _memory.Remove(MemoryKey(languageTag));
        await _distributed.RemoveAsync(languageTag);
    }

    private void SetMemory(string memKey, Dictionary<string, string> map)
    {
        _memory.Set(memKey, map, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.MemoryTtl
        });
    }

    private async Task<Dictionary<string, string>> LoadPopulateAsync(string languageTag)
    {
        using var scope = _scopeFactory.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<ITranslationSource>();
        
        var map = await source.LoadAsync(languageTag);

        await _distributed.SetAsync(languageTag, map, _options.DistributedTtl);

        return map;
    }
}