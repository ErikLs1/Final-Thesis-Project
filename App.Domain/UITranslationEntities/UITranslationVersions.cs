using App.Domain.Enum;

namespace App.Domain.UITranslationEntities;

public class UITranslationVersions
{
    public Guid Id { get; set; }
    public Guid LanguageId { get; set; }
    public Guid ResourceKeyId { get; set; }
    public TranslationState TranslationState { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; } 
    public string CreatedBy { get; set; } = null!;

    public Languages Language { get; set; } = null!;
    public UIResourceKeys UIResourceKeys { get; set; } = null!;
    public ICollection<UITranslationAuditLog> UITranslationAuditLogs { get; set; }= new List<UITranslationAuditLog>();
    
}