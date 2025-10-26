using App.Repository.DTO;

namespace App.Repository.Interface;

public interface IUserLanguageRepository
{
    Task<IReadOnlyList<Guid>> GetLanguageIdsByUserAsync(Guid userId);
    Task<IReadOnlyList<UserKnownLanguagesDto>> GetUserKnownLanguagesAsync(Guid userId);
    Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languagesIds);
}