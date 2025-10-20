using App.Repository.DalUow;
using App.Repository.DTO;
using App.Service.Interface;

namespace App.Service.Impl;

public class LanguageService : ILanguageService
{
    private readonly IAppUow _uow;
    
    public LanguageService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public Task<IReadOnlyList<LanguageDto>> GetAllLanguages()
    {
        return _uow.LanguageRepository.GetAllLanguagesAsync();
    }
}