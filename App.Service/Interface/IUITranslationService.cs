using App.Repository.DTO;
using App.Repository.DTO.UITranslations;
using WebApp.Extensions.Pager.models;

namespace App.Service.Interface;

public interface IUITranslationService
{
    Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId);
    Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request);
    Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag);

    Task<PagedResult<FilteredUITranslationsDto>> GetFilteredUITranslationsAsync(
        FilteredTranslationsRequestDto request,
        PagedRequest paging);
    Task<int> PublishTranslationTranslationsAsync(IReadOnlyList<PublishTranslationVersionRequestDto> requests);
}