using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace LoyaltySphere.RewardService.Infrastructure.Caching;

/// <summary>
/// Custom Redis health check that pings the connected Redis instance.
/// Uses the registered IConnectionMultiplexer singleton.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _multiplexer;

    public RedisHealthCheck(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_multiplexer.IsConnected)
                return HealthCheckResult.Degraded("Redis is not connected but may reconnect.");

            var db = _multiplexer.GetDatabase();
            var latency = await db.PingAsync();

            return HealthCheckResult.Healthy($"Redis is reachable (latency: {latency.TotalMilliseconds:F1}ms)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis is unreachable", ex);
        }
    }
}
