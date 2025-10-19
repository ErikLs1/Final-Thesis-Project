using App.Repository.DalUow;
using App.Service.Interface;

namespace App.Service.Impl;

public class UIExperimentService : IUIExperimentService
{
    private readonly IAppUow _uow;
    
    public UIExperimentService(IAppUow serviceUow)
    {
        _uow = serviceUow;
    }
}