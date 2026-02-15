using App.Repository.DalUow;
using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using App.Repository.Pager;
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

    public Task<PagedResult<FilteredUITranslationsDto>> GetFilteredUITranslationsAsync(
        FilteredTranslationsRequestDto request,
        PagedRequest paging)
    {
        return _uow.UITranslationRepository.GetFilteredUITranslationsAsync(request, paging);
    }

    public async Task<int> PublishTranslationTranslationsAsync(IReadOnlyList<PublishTranslationVersionRequestDto> requests)
    {
        var sum = 0;
        foreach (var version in requests)
        {
            var singleReq = new PublishTranslationVersionRequestDto(
                version.TranslationVersionId,
                version.ActivatedBy
            );

            var changed = await _uow.UITranslationRepository.PublishTranslationVersionAsync(singleReq);
            sum += changed;
        }

        return sum;
    }
}