namespace WebApp.Models.DynamicTranslations;

public class IndexVm
{
    public string LanguageTag { get; set; } = "en";
    public Dictionary<string, string> Translation { get; set; } = new();

    public string Value(string key)
    {
        return Translation.TryGetValue(key, out var v) ? v : key;
    }
}