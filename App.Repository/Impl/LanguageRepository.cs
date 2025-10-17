using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _db;
    
    public LanguageRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}