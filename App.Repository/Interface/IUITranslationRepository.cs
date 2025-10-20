using App.Repository.DTO;

namespace App.Repository.Interface;

public interface IUITranslationRepository
{
    Task<IReadOnlyList<LiveTranslationDto>> GetLiveTranslationsAsync(Guid? languageId);
    Task<int> UpdateTranslationStateAsync(UpdateTranslationStateRequestDto request);

    Task<Dictionary<string, string>> GetLiveTranslationsByLanguageTagAsync(string languageTag);
}