namespace WebApp.Extensions.Configuration;

public class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en";
    public List<string> SupportedCultures { get; set; } = new();
}