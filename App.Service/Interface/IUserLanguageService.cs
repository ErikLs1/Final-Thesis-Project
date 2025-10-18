using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUserLanguageService
{
    Task<IReadOnlyList<Guid>> GetUserLanguageIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<LanguageDto>> GetAllLanguages(CancellationToken ct = default); // TODO: Move to Language Service
    Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languageIds, CancellationToken ct = default);
}