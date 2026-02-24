namespace App.EF;

public interface ICurrentUserProvider
{
    string? GetCurrentUserName();
}
