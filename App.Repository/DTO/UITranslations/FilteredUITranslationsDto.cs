using App.Domain.Enum;

namespace App.Repository.DTO.UITranslations;

public record FilteredUITranslationsDto(
    Guid LanguageId,
    string LanguageTag,
    Guid ResourceKeyId,
    string ResourceKey,
    string FriendlyKey,
    Guid TranslationVersionId,
    int VersionNumber,
    string Content,
    TranslationState TranslationState
);