using App.Repository.DalUow;
using App.Repository.DTO;
using App.Service.Interface;

namespace App.Service.Impl;

public class UITranslationsVersionsService : IUITranslationsVersionsService
{
    private readonly IAppUow _uow;
    
    public UITranslationsVersionsService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public async Task<IReadOnlyList<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync()
    {
        var defaultLanguageId = await _uow.LanguageRepository.GetDefaultLanguageIdAsync();
        return await _uow.UITranslationsVersionsRepository.GetDefaultLanguageTranslationsAsync(defaultLanguageId);
    }

    public async Task<IReadOnlyList<TranslationVersionRowDto>> GetFilteredTranslationsAsync(Guid? languageId, int? version)
    {
        return await _uow.UITranslationsVersionsRepository.GetTranslationVersionAsync(languageId, version);
    }

    public async Task<int> CreateTranslationVersionsAsync(CreateVersionRequestDto request)
    {
        return await _uow.UITranslationsVersionsRepository.CreateNewVersionAsync(request);
    }
}