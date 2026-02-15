namespace WebApp.Helpers.Translations.Options;

// TODO: CHANGE LATER - translations might live longer
// TODO: MOVE ALL CACHING LOGIC TO THE BLL
public sealed class TranslationCacheOptions
{
    public string RedisKeyPrefix { get; init; } = "ui:translations";
    public TimeSpan DistributedTtl { get; init; } = TimeSpan.FromHours(6);
    public TimeSpan MemoryTtl { get; init; } = TimeSpan.FromMinutes(10);
}