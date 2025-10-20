using App.Repository.DTO;

namespace App.Repository.Interface;

public interface IUITranslationsVersionsRepository
{
    Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(Guid? languageId);
    Task<IReadOnlyList<TranslationVersionRowDto>> GetTranslationVersionAsync(Guid? languageId, int? version);
    Task<int> CreateNewVersionAsync(CreateVersionRequestDto request);
}