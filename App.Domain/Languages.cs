using App.Domain.AB;
using App.Domain.Identity;
using App.Domain.UITranslationEntities;

namespace App.Domain;

public class Languages
{
    public Guid Id { get; set; }
    public string LanguageTag { get; set; } = null!;
    public string LanguageName { get; set; } = null!;
    public bool IsDefaultLanguage { get; set; }

    public ICollection<UserLanguages> UserLanguages { get; set; }= new List<UserLanguages>();
    public ICollection<UITranslationAuditLog> UITranslationAuditLogs { get; set; }= new List<UITranslationAuditLog>();
    public ICollection<UITranslationVersions> UITranslationVersions { get; set; }= new List<UITranslationVersions>();
    public ICollection<UIExperiment> UIExperiments { get; set; }= new List<UIExperiment>();
    public UITranslations UITranslations { get; set; } = null!; // Perform check to accept only one active translation
}