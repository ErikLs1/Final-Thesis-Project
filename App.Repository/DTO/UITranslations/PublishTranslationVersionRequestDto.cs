namespace App.Repository.DTO.UITranslations;

public record PublishTranslationVersionRequestDto(
    Guid TranslationVersionId,
    string ActivatedBy
);