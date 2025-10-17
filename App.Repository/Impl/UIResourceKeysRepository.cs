using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UIResourceKeysRepository :IUIResourceKeysRepository
{
    public UIResourceKeysRepository(AppDbContext repositoryDbContext)
    {
    }
}