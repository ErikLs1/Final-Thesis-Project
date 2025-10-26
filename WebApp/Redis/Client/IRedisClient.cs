using IDatabase = StackExchange.Redis.IDatabase;

namespace WebApp.Redis.Client;

public interface IRedisClient
{
    IDatabase GetDb();
}