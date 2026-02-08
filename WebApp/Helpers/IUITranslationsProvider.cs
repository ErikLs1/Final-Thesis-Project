namespace WebApp.Helpers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/make-content-localizable?view=aspnetcore-9.0
public interface IUITranslationsProvider
{
    /// <summary>
    /// The BCP-47 language tag of the resource.
    /// </summary>
    string LanguageTag { get; }
    Task<string> GetAsync(string key);
}