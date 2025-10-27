using App.Domain.Enum;

namespace App.Repository.DTO.UITranslations;

public record FilteredTranslationsRequestDto(
    Guid LanguageId,
    int? VersionNumber,
    TranslationState? State
);