using App.Domain.Enum;

namespace App.Repository.DTO;

public record LiveTranslationDto(   
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