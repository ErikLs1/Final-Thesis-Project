using App.Domain.Enum;

namespace App.Repository.DTO;

public record UpdateTranslationStateRequestDto(    
    Guid TranslationVersionId,
    TranslationState NewState,
    string UpdatedBy
);