using App.Repository.DTO;

namespace App.Repository.Interface;

public interface ILanguageRepository
{
    Task<IReadOnlyList<LanguageDto>> GetAllLanguagesAsync(CancellationToken ct = default);
    Task<Guid> GetDefaultLanguageIdAsync(CancellationToken ct = default);
}