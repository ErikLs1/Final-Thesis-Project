namespace WebApp.Helpers;

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/make-content-localizable?view=aspnetcore-9.0
public interface IUITranslationsProvider
{
    /// <summary>
    /// The BCP-47 language tag of the resource.
    /// </summary>
    string LanguageTag { get; }
    
    /// <summary>
    /// The key of the string resource
    /// </summary>
    /// <param name="key"></param>
    string this[string key] { get; } // @Translation["Translation_string_code"]
    
    /// <summary>
    /// Gets specific key translation resource
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string Get(string key);
}