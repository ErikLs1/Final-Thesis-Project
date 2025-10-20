using App.Repository.DTO;

namespace App.Service.Interface;

public interface IUITranslationService
{
    Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId);
    Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request);
    Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag);
}