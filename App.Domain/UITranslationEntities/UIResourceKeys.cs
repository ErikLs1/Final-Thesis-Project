namespace App.Domain.UITranslationEntities;

public class UIResourceKeys
{
    public Guid Id { get; set; }
    public string ResourceKey { get; set; } = null!; // unique
    public string FriendlyKey { get; set; } = null!;

    public ICollection<UITranslationAuditLog> UITranslationAuditLogs { get; set; }= new List<UITranslationAuditLog>();
    public ICollection<UITranslationVersions> UITranslationVersions { get; set; }= new List<UITranslationVersions>();
    public ICollection<UITranslations> UITranslations { get; set; } = new List<UITranslations>();
}