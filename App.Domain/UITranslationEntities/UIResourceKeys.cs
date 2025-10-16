using App.Domain.AB;

namespace App.Domain.UITranslationEntities;

public class UIResourceKeys
{
    public Guid Id { get; set; }
    public string ResourceKey { get; set; } = null!; // unique
    public string Content { get; set; } = null!;

    public ICollection<UITranslationAuditLog> UITranslationAuditLogs { get; set; }= new List<UITranslationAuditLog>();
    public ICollection<UITranslationVersions> UITranslationVersions { get; set; }= new List<UITranslationVersions>();
    public ICollection<UIExperiment> UIExperiments { get; set; }= new List<UIExperiment>();
    public UITranslations UITranslations { get; set; } = null!; // Perform check to accept only one key
}