using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUITranslationsVersionsService
{
    Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync();
    Task<IReadOnlyList<TranslationVersionRowDto>> GetFilteredTranslationsAsync(Guid? languageId, int? version);
    Task<int> CreateTranslationVersionsAsync(CreateVersionRequestDto request);
}