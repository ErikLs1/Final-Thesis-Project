using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationsVersionsRepository : IUITranslationsVersionsRepository
{
    public UITranslationsVersionsRepository(AppDbContext repositoryDbContext)
    {
    }
}