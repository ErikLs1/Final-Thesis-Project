using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUITranslationsVersionsService
{
    Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(CancellationToken ct);
    Task<IReadOnlyList<TranslationVersionRowDto>> GetFilteredTranslationsAsync(Guid? languageId, int? version, CancellationToken ct);
    Task<int> CreateTranslationVersionsAsync(CreateVersionRequestDto request, CancellationToken ct);
}