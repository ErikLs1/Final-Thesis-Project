using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class UIExperimentRepository : IUIExperimentRepository
{
    private readonly AppDbContext _db;
    
    public UIExperimentRepository(AppDbContext repositoryDbContext)
    {
        _db = repositoryDbContext;
    }
}