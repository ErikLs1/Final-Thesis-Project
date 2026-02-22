using App.Domain.Enum;

namespace App.Repository.DTO.UITranslations;

public record TranslationAuditRowDto(
    DateTime ChangedAt,
    string ChangedBy,
    TranslationAuditAction ActionType,
    Guid LanguageId,
    string LanguageTag,
    Guid ResourceKeyId,
    string ResourceKey,
    string FriendlyKey,
    Guid TranslationVersionId,
    int VersionNumber,
    TranslationState? OldState,
    TranslationState? NewState,
    string? OldContent,
    string? NewContent
);
