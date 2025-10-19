using App.Repository.DalUow;
using App.Service.Interface;

namespace App.Service.Impl;

public class UIResourceKeysService : IUIResourceKeysService
{
    private readonly IAppUow _uow;
    
    public UIResourceKeysService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }
}