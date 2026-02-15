using App.Repository.DTO;
using App.Repository.Pager;

namespace App.Repository.Interface;

public interface IUITranslationsVersionsRepository
{
    Task<PagedResult<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(
        Guid? languageId,
        PagedRequest paging,
        string? keySearch = null);
    
    Task<PagedResult<TranslationVersionRowDto>> GetTranslationVersionAsync(
        Guid? languageId, 
        int? version,
        PagedRequest paging,
        string? keySearch = null);
    
    Task<int> CreateNewVersionAsync(CreateVersionRequestDto request);
}