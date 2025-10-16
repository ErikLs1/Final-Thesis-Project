namespace App.Domain.UITranslationEntities;

public class UITranslations
{
    public Guid Id { get; set; }
    public Guid LanguageId { get; set; }
    public Guid ResourceKeyId { get; set; }
    public Guid TranslationVersionId { get; set; }
    public DateTime PublishedAt { get; set; } 
    public string PublishedBy { get; set; } = null!;

    public Languages Language { get; set; } = null!;
    public UIResourceKeys UIResourceKeys { get; set; } = null!;
    public UITranslationVersions UITranslationVersions { get; set; } = null!;
}