namespace WebApp.Models;

public class MyLanguagesVm
{
    public List<LanguageRow> Selected { get; set; } = new();
    public bool HasAny => Selected.Count > 0;
}