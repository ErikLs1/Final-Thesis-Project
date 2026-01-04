using App.Repository.Impl.ResxImport;
using App.Service.Impl.Assemblies.Importer;

namespace WebApp.Extensions.Builder;

public static class ResxImportBuilderExtension
{
    public static async Task RunResxImportAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var assemblyImporter = scope.ServiceProvider.GetRequiredService<ResourcesImporter>();
            await assemblyImporter.ImportFromAssembliesAsync();
        }
        catch (Exception e)
        {
            app.Logger.LogWarning(e, "ASSEMBLY RESX IMPORT FAILED");
        }
    }
}