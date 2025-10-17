using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UserLanguageRepository : IUserLanguageRepository
{
    private readonly AppDbContext _db;
    
    public UserLanguageRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}