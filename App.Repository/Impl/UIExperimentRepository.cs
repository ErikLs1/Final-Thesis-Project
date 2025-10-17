using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UIExperimentRepository : IUIExperimentRepository
{
    public UIExperimentRepository(AppDbContext repositoryDbContext)
    {
    }
}