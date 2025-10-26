using StackExchange.Redis;
using IDatabase = Microsoft.EntityFrameworkCore.Storage.IDatabase;

namespace WebApp.Redis.Client.Impl;

public class RedisClient : IRedisClient
{
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisClient()
    {
        var configuration = new ConfigurationOptions
        {
            EndPoints = { "localhost:6379" },
            User = "yourUsername",
            Password = "yourPassword"
        };

        _multiplexer = ConnectionMultiplexer.Connect(configuration);
    }

    public StackExchange.Redis.IDatabase GetDb()
    {
        return _multiplexer.GetDatabase();
    }

    public void Dispose()
    {
        _multiplexer.Dispose();
    }
}