using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.DependencyModel;
using WebApp.Vol2.Utils;

namespace WebApp.Vol2.Base;

public class BaseAssembly
{
    // All project with Resx files
    private static readonly HashSet<string> ResourceAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        AssemblyUtils.AppDataAssembly,
        // AssemblyUtils.AppCommonAssembly,
    };
    
    public static IEnumerable<Assembly> GetAssembliesFromDependencyContext()
    {
        var context = DependencyContext.Default 
                      ?? throw new InvalidOperationException("DependencyContext does not exist.");

        var assemblies = new List<Assembly>();
        
        foreach (var library in context.RuntimeLibraries)
        {
            if (!ResourceAssemblyNames.Contains(library.Name))
            {
                continue;
            }

            try
            {
                var assembly = Assembly.Load(new AssemblyName(library.Name));
                assemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load resource assembly '{library.Name}'.", ex);
            }
        }

        return assemblies;
    }

    public static bool IsResxDesignerClass(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        var  resourceManagerProp = type.GetProperty(
            AssemblyUtils.ResourceManager,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        var cultureInfoProp = type.GetProperty(
            AssemblyUtils.Culture,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        return resourceManagerProp?.PropertyType == typeof(ResourceManager)
               && cultureInfoProp?.PropertyType == typeof(CultureInfo);
    }


    public static ResourceManager? GetResourceManager(Type resourceType)
    {
        var rmProp = resourceType.GetProperty(
            AssemblyUtils.ResourceManager,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        return rmProp?.GetValue(null) as ResourceManager;
    }
}