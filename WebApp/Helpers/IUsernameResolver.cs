namespace WebApp.Helpers;

public interface IUserNameResolver
{
    string CurrentUserName { get; }
}