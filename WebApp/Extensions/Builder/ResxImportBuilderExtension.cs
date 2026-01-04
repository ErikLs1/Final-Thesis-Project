using App.Repository.Impl.ResxImport;
using App.Repository.Interface;
using Microsoft.Extensions.Options;
using WebApp.Extensions.Configuration;

namespace WebApp.Extensions.Builder;

public static class ResxImportBuilderExtension
{
    public static async Task RunResxImportAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var importerService = scope.ServiceProvider.GetRequiredService<ResxImportRepository>();
        
        var path = app.Configuration["Resx:LocalResourcesPath"];
        var resxFolder = string.IsNullOrWhiteSpace(path) ? "../App.Data/Resources" : path;

        if (!Path.IsPathRooted(resxFolder))
        {
            resxFolder = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, resxFolder));
        }
        
        if (!Directory.Exists(resxFolder))
        {
            app.Logger.LogWarning("RESX folder not found. Import skipped: {Path}", resxFolder);
            return;
        }
        
        // Import initial or missing resx keys and first version to UIResourceKeys and UITranslationVersions table
        await importerService.ImportFirstTranslationVersionAsync(resxFolder);
        
        // Insert first ever translation versions to UITanslations table
        await importerService.InialUITranslationsImportAsync("resx-startup-import");
    }
}