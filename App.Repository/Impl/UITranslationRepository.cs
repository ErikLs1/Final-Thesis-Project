using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UITranslationRepository : IUITranslationRepository
{
    private readonly AppDbContext _db;
    
    public UITranslationRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}