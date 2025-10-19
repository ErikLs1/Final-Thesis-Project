using App.Repository.DalUow;
using App.Service.Interface;

namespace App.Service.Impl;

public class UITranslationService : IUITranslationService
{
    private readonly IAppUow _uow;
    
    public UITranslationService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }
}