using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UIResourceKeysRepository :IUIResourceKeysRepository
{
    private readonly AppDbContext _db;
    
    public UIResourceKeysRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}