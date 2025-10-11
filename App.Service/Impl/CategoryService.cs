using App.Repository;
using App.Repository.DalUow;
using App.Service.Interface;

namespace App.Service.Impl;

public class CategoryService : ICategoryService
{
    public CategoryService(IAppUow serviceUow)
    {
    }
}