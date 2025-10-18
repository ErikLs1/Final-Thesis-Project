namespace App.Repository.Interface;

public interface IUserLanguageRepository
{
    Task<IReadOnlyList<Guid>> GetLanguageIdsByUserAsync(Guid userId, CancellationToken ct = default);
    Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languagesIds, CancellationToken ct = default);
}