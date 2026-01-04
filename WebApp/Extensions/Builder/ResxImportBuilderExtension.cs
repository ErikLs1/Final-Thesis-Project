using App.Repository.Impl.ResxImport;
using WebApp.Vol2.Importer;

namespace WebApp.Extensions.Builder;

public static class ResxImportBuilderExtension
{
    public static async Task RunResxImportAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var assemblyImporter = scope.ServiceProvider.GetRequiredService<ResourcesImporter>();
            await assemblyImporter.ImportFromAssembliesAsync(
                publishedBy: "resx-startup-import",
                createdBy: "resx-import");
        }
        catch (Exception e)
        {
            app.Logger.LogWarning(e, "ASSEMBLY RESX IMPORT FAILED");
        }
    }
}