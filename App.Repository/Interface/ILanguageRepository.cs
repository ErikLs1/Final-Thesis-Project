using App.Repository.DTO;

namespace App.Repository.Interface;

public interface ILanguageRepository
{
    Task<IReadOnlyList<LanguageDto>> GetAllLanguagesAsync();
    Task<Guid> GetDefaultLanguageIdAsync();
}