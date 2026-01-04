using Microsoft.Extensions.Options;
using StackExchange.Redis;
namespace WebApp.Redis.Client.Impl;

public class RedisClient : IRedisClient
{
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisClient(IOptions<RedisOptions> options)
    {
        var o = options.Value;
        
        var configuration = ConfigurationOptions.Parse(o.ConnectionString);
        configuration.AbortOnConnectFail = false;

        if (!string.IsNullOrWhiteSpace(o.User)) configuration.User = o.User;
        if (!string.IsNullOrWhiteSpace(o.Password)) configuration.Password = o.Password;
        

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