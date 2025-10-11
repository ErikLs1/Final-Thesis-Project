namespace App.Repository;

public interface IBaseUow
{
    public Task<int> SaveChangesAsync();
}