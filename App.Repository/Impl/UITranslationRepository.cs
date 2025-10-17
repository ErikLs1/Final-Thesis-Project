using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationRepository : IUITranslationRepository
{
    public UITranslationRepository(AppDbContext repositoryDbContext)
    {
    }
}