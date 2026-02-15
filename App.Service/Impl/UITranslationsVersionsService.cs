using App.Repository.DalUow;
using App.Repository.DTO;
using App.Service.Interface;
using WebApp.Extensions.Pager.models;

namespace App.Service.Impl;

public class UITranslationsVersionsService : IUITranslationsVersionsService
{
    private readonly IAppUow _uow;
    
    public UITranslationsVersionsService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }

    public async Task<PagedResult<TranslationVersionRowDto>> GetDefaultLanguageTranslationsAsync(PagedRequest paging, string? keySearch = null)
    {
        var defaultLanguageId = await _uow.LanguageRepository.GetDefaultLanguageIdAsync();
        return await _uow.UITranslationsVersionsRepository
            .GetDefaultLanguageTranslationsAsync(defaultLanguageId, paging, keySearch);
    }

    public async Task<PagedResult<TranslationVersionRowDto>> GetFilteredTranslationsAsync(Guid languageId, int? version, PagedRequest paging, string? keySearch = null)
    {
        return await _uow.UITranslationsVersionsRepository
            .GetTranslationVersionAsync(languageId, version, paging, keySearch);
    }

    public async Task<int> CreateTranslationVersionsAsync(CreateVersionRequestDto request)
    {
        return await _uow.UITranslationsVersionsRepository.CreateNewVersionAsync(request);
    }
}