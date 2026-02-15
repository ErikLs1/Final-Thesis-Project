using App.Repository.DTO;
using App.Repository.Pager;

namespace App.Service.Interface;

public interface IUITranslationsVersionsService
{
    Task<PagedResult<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(
        PagedRequest paging,
        string? keySearch = null);

    Task<PagedResult<TranslationVersionRowDto>> GetFilteredTranslationsAsync(
        Guid languageId,
        int? version,
        PagedRequest paging,
        string? keySearch = null);
    
    Task<int> CreateTranslationVersionsAsync(CreateVersionRequestDto request);
}