using App.Repository.Interface;

namespace App.Repository;

public interface IAppUow : IBaseUow
{
    IProductRepository ProductRepository { get; }
}