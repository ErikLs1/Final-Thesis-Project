using System.Collections;
using System.Globalization;
using System.Resources;
using WebApp.Vol2.Base;

namespace WebApp.Vol2;

public static class ResourcesScanner
{
    // https://www.nuget.org/packages/Microsoft.Extensions.DependencyModel
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencymodel.dependencycontext?view=net-9.0-pp
    // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencymodel.runtimelibrary?view=net-9.0-pp
    
    
    public static void AllAssemblyResources(ILogger logger)
    {
        var assemblies = BaseAssembly.GetAssembliesFromDependencyContext().ToList();
        
        foreach (var assembly in assemblies)
        {
            var assemblyName = assembly.GetName().Name ?? "UNKNOWN";
            
            foreach (var type in assembly.GetTypes())
            {
                if (!BaseAssembly.IsResxDesignerClass(type))
                  continue;

                var resourceManager = BaseAssembly.GetResourceManager(type);
                if (resourceManager == null)
                    continue;

                logger.LogInformation("=== Resource type {Type}. Assembly {Asm} ===",
                    type.FullName, assemblyName);

                // TODO: LATER. GET ALL RESOURCES AND IMPORT DB
                // 1) base RESX
                TranslationsSet(logger, resourceManager, CultureInfo.InvariantCulture, "neutral");

                // 2) App Cultures
                foreach (var cultureName in new[] { "en-US", "et-EE", "ru", "de-CH", "es-ES" })
                {
                    TranslationsSet(logger, resourceManager, new CultureInfo(cultureName), cultureName);
                }
            }
        }
    }

    private static void TranslationsSet(ILogger logger, ResourceManager resourceManager, CultureInfo cultureInfo, string label)
    {
        var set = resourceManager.GetResourceSet(cultureInfo, createIfNotExists: true, tryParents: false);
        if (set == null)
        {
            logger.LogInformation(" -- NO RESOURCES FOUND FOR CULTURE - {Label}", label);
            return;
        }

        logger.LogInformation("  --- RESOURCES FOR {Label} ---", label);

        foreach (DictionaryEntry entry in set)
        {
            if (entry.Key is string key)
            {
                var value = entry.Value?.ToString() ?? string.Empty;
                logger.LogInformation("  [{Label}] {Key} = {Value}", label, key, value);
            }
        }
    }
}