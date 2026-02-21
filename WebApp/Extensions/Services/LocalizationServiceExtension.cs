using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using WebApp.Extensions.Configuration;

namespace WebApp.Extensions.Services;

public static class LocalizationServiceExtension
{
    public static IServiceCollection AddAppLocalization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // get localization options
        services.Configure<AppLocalizationOptions>(
            configuration.GetSection("Localization")
        );
        
        // add resx localization
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        
        
        // RequestLocalizationOptions config
        services.AddSingleton<IConfigureOptions<RequestLocalizationOptions>>(x =>
        {
            return new ConfigureOptions<RequestLocalizationOptions>(options =>
            {
                var localizationOptions = x
                    .GetRequiredService<IOptions<AppLocalizationOptions>>()
                    .Value;

                var defaultCulture = string.IsNullOrWhiteSpace(localizationOptions.DefaultCulture)
                    ? "en"
                    : localizationOptions.DefaultCulture;

                var supportedCultures = localizationOptions.SupportedCultures
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => new CultureInfo(c))
                    .ToList();
                
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                
                //Fallbacks
                options.FallBackToParentCultures = true; 
                options.FallBackToParentUICultures = true;
                
                // strategy for determining culture
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                { 
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };
            });

        });

        return services;
    }
}