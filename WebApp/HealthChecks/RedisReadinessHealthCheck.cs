using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApp.Redis.Client;

namespace WebApp.HealthChecks;

public sealed class RedisReadinessHealthCheck : IHealthCheck
{
    private readonly IRedisClient _redisClient;

    public RedisReadinessHealthCheck(IRedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await _redisClient.GetDb().PingAsync();
            return HealthCheckResult.Healthy("Redis is reachable.", new Dictionary<string, object?>
            {
                ["latency_ms"] = latency.TotalMilliseconds
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis health check failed.", ex);
        }
    }
}
