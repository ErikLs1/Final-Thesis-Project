using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUserLanguageService
{
    Task<IReadOnlyList<Guid>> GetUserLanguageIdsAsync(Guid userId, CancellationToken ct = default);
    Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languageIds, CancellationToken ct = default);
}