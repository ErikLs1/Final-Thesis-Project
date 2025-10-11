using App.Domain;
using App.EF;
using App.Repository.DalUow;
using App.Repository.Interface;

namespace App.Repository.Impl;

public class CategoryRepository : BaseRepository, ICategoryRepository
{
    public CategoryRepository(AppDbContext repositoryDbContext) : base(repositoryDbContext)
    {
    }
}