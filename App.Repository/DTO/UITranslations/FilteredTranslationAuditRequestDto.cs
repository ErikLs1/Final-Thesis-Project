using App.Domain.Enum;

namespace App.Repository.DTO.UITranslations;

public record FilteredTranslationAuditRequestDto(
    Guid? LanguageId,
    TranslationAuditAction? ActionType,
    string? ChangedBy,
    string? ResourceKeySearch,
    DateTime? ChangedFromUtc,
    DateTime? ChangedToUtc
);
