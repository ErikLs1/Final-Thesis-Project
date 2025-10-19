using App.Repository.DTO;

namespace App.Repository.Interface;

public interface IUITranslationsVersionsRepository
{
    Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(Guid? languageId, CancellationToken ct = default);
    Task<IReadOnlyList<TranslationVersionRowDto>> GetTranslationVersionAsync(Guid? languageId, int? version,
        CancellationToken ct = default);
    Task<int> CreateNewVersionAsync(CreateVersionRequestDto request, CancellationToken ct = default);
}