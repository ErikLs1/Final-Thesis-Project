using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UserLanguageRepository : IUserLanguageRepository
{
    public UserLanguageRepository(AppDbContext repositoryDbContext)
    {
    }
}