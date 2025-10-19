using App.Repository.DTO;

namespace App.Service.Interface;

public interface ILanguageService
{
    Task<IReadOnlyList<LanguageDto>> GetAllLanguages(CancellationToken ct = default);

}