using App.EF;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class ResxImportRepository : IResxImportRepository
{
    public ResxImportRepository(AppDbContext repositoryDbContext)
    {
    }
}