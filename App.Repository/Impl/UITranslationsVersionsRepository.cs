using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationsVersionsRepository : IUITranslationsVersionsRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationsVersionsRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}