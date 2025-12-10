using System.Collections.ObjectModel;
using System.Resources;
using WebApp.Vol2.Base;

namespace WebApp.Vol2.Resx;

public class ResourceManagerRegistry
{
    public static IReadOnlyList<ResourceManager> All => GetAllResourceManagers();
    private static IReadOnlyList<ResourceManager> GetAllResourceManagers()
    {
        var result = new List<ResourceManager>();

        var assemblies = BaseAssembly.GetAssembliesFromDependencyContext();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!BaseAssembly.IsResxDesignerClass(type))
                    continue;

                var resourceManager = BaseAssembly.GetResourceManager(type);
                if (resourceManager != null)
                {
                    result.Add(resourceManager);
                }
            }
        }

        return new ReadOnlyCollection<ResourceManager>(result);
    }
}