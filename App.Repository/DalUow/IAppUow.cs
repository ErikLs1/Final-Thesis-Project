using App.Repository.Interface;

namespace App.Repository.DalUow;

public interface IAppUow : IBaseUow
{
    IProductRepository ProductRepository { get; }
    ICategoryRepository CategoryRepository { get; }
}