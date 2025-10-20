using App.Repository.DalUow;
using App.Repository.DTO;
using App.Service.Interface;

namespace App.Service.Impl;

public class UITranslationService : IUITranslationService
{
    private readonly IAppUow _uow;
    
    public UITranslationService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public async Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId)
    {
        return await _uow.UITranslationRepository.GetLiveTranslationsAsync(languageId);
    }

    public async Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request)
    {
        return await _uow.UITranslationRepository.UpdateTranslationStateAsync(request);
    }

    public async Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag)
    {
        return await _uow.UITranslationRepository.GetLiveTranslationsByLanguageTagAsync(languageTag);
    }
}