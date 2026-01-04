using System.Collections.ObjectModel;
using System.Resources;
using App.Service.Impl.Assemblies.Base;

namespace App.Service.Impl.Assemblies.Resx;

public class ResourceManagerRegistry
{
    private static readonly Lazy<IReadOnlyList<ResourceManager>> _all =
        new (GetAllResourceManagers);

    // Cached managers (scans for managers only once and then uses them around the app)
    public static IReadOnlyList<ResourceManager> All => _all.Value;
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