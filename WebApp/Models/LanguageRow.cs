namespace WebApp.Models;

public class LanguageRow
{
    public Guid Id { get; set; }
    public string Tag { get; set; } = default!;
    public string Name { get; set; } = default!;
}