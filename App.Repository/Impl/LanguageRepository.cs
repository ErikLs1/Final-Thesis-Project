using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class LanguageRepository : ILanguageRepository
{
    public LanguageRepository(AppDbContext repositoryDbContext)
    {
    }
}