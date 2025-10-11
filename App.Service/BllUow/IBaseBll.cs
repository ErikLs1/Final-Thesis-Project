namespace App.Service;

public interface IBaseBll
{
    public Task<int> SaveChangesAsync();
}