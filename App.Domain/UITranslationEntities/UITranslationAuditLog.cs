namespace App.Domain.UITranslationEntities;

public class UITranslationAuditLog
{
    public Guid Id { get; set; }
    public Guid LanguageId { get; set; }
    public Guid ResourceKeyId { get; set; }
    public Guid TranslationVersionId { get; set; }
    public DateTime ActivatedAt { get; set; } 
    public string ActivatedBy { get; set; } = null!;
    public DateTime DeactivatedAt { get; set; } 
    public string DeactivatedBy { get; set; } = null!;

    public Languages Language { get; set; } = null!;
    public UIResourceKeys UIResourceKeys { get; set; } = null!;
    public UITranslationVersions UITranslationVersions { get; set; } = null!;
}