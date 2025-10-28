namespace WebApp.Extensions.Builder;

public static class LocalizationBuilderExtension
{
    public static IApplicationBuilder UseLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();
        return app;
    }
}