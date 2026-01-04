using System.Collections;
using System.Globalization;
using System.Resources;
using WebApp.Vol2.Resx;

namespace WebApp.Vol2.Scanner;

public static class ResourcesScanner
{
    // https://www.nuget.org/packages/Microsoft.Extensions.DependencyModel
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencymodel.dependencycontext?view=net-9.0-pp
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencymodel.runtimelibrary?view=net-9.0-pp

    public static IReadOnlyDictionary<string, string> ReadEntries(
        ResourceManager resourceManager,
        CultureInfo cultureInfo,
        bool tryParents = false)
    {
        var set = resourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: tryParents);
        if (set == null) return new Dictionary<string, string>(0);

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in set)
        {
            if (entry.Key is not string key || string.IsNullOrWhiteSpace(key)) continue;
            dict[key] = entry.Value?.ToString() ?? string.Empty;
        }

        return dict;
    }
    
    public static IReadOnlyDictionary<string, string> AggregateEntries(
        CultureInfo culture,
        ILogger? logger = null,
        bool tryParents = false)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var rm in ResourceManagerRegistry.All)
        {
            var entries = ReadEntries(rm, culture, tryParents);
            foreach (var (key, value) in entries)
            {
                if (merged.TryGetValue(key, out var existing) && existing != value)
                {
                    logger?.LogWarning(
                        "RESX key collision for culture {Culture}: '{Key}' overwritten (BaseName={BaseName})",
                        culture.Name, key, rm.BaseName);
                }

                merged[key] = value;
            }
        }

        return merged;
    }
}