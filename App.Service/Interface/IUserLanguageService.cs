using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUserLanguageService
{
    Task<IReadOnlyList<Guid>> GetUserLanguageIdsAsync(Guid userId);
    Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languageIds);
}