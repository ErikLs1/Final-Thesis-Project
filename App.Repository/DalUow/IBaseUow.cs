namespace App.Repository.DalUow;

public interface IBaseUow
{
    public Task<int> SaveChangesAsync();
}