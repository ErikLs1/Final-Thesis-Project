using App.Repository.Impl.ResxImport;
using Microsoft.Extensions.Options;
using WebApp.Extensions.Configuration;

namespace WebApp.Extensions.Builder;

public static class ResxImportBuilderExtension
{
    public static async Task RunResxImportAsync(this WebApplication app)
    {
        // Get Resx import config from appsettings.json
        var options = app.Services
            .GetRequiredService<IOptions<ResxImportOptions>>()
            .Value;

        // Return early if config says so
        if (!options.ImportOnStartup || string.IsNullOrWhiteSpace(options.Folder))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var importerService = scope.ServiceProvider.GetRequiredService<ResxImportRepository>();
        
        // Import initial or missing resx keys and first version to UIResourceKeys and UITranslationVersions table
        await importerService.ImportFirstTranslationVersionAsync(options.Folder);
        
        // Insert first ever translation versions to UITanslations table
        await importerService.InialUITranslationsImportAsync("resx-startup-import");
    }
}