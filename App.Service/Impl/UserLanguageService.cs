using App.Repository.DalUow;
using App.Repository.DTO;
using App.Service.Interface;

namespace App.Service.Impl;

public class UserLanguageService : IUserLanguageService
{
    private readonly IAppUow _uow;
    
    public UserLanguageService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public Task<IReadOnlyList<Guid>> GetUserLanguageIdsAsync(Guid userId)
    {
        return _uow.UserLanguageRepository.GetLanguageIdsByUserAsync(userId);
    }

    public Task UpdateUserLanguagesAsync(Guid userId, IEnumerable<Guid> languageIds)
    {
        return _uow.UserLanguageRepository.UpdateUserLanguagesAsync(userId, languageIds);

    }
}