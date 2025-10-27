using App.Repository.DTO;
using App.Repository.DTO.UITranslations;

namespace App.Service.Interface;

public interface IUITranslationService
{
    Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId);
    Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request);
    Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag);
    Task<IReadOnlyList<FilteredUITranslationsDto>> GetFilteredUITranslationsAsync(
        FilteredTranslationsRequestDto request
    );
}