using System.Text.Json;
using LoyaltySphere.MultiTenancy;
using LoyaltySphere.RewardService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace LoyaltySphere.RewardService.Infrastructure.Caching;

/// <summary>
/// Redis-backed cache service with tenant-aware key prefixing and graceful degradation.
/// Cache failures are logged but never propagated — a cache miss is always safe.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(
        IDistributedCache cache,
        ITenantContext tenantContext,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var prefixedKey = BuildKey(key);
            var data = await _cache.GetStringAsync(prefixedKey, cancellationToken);

            if (data == null)
            {
                _logger.LogDebug("Cache MISS for key {CacheKey}", prefixedKey);
                return null;
            }

            _logger.LogDebug("Cache HIT for key {CacheKey}", prefixedKey);
            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache GET failed for key {CacheKey}. Degrading to cache miss.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var prefixedKey = BuildKey(key);
            var data = JsonSerializer.Serialize(value, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(prefixedKey, data, options, cancellationToken);
            _logger.LogDebug("Cache SET for key {CacheKey}, TTL {Expiration}", prefixedKey, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key {CacheKey}. Data will be fetched from DB on next read.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var prefixedKey = BuildKey(key);
            await _cache.RemoveAsync(prefixedKey, cancellationToken);
            _logger.LogDebug("Cache REMOVE for key {CacheKey}", prefixedKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache REMOVE failed for key {CacheKey}.", key);
        }
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // IDistributedCache doesn't support prefix deletion natively.
        // For production, you'd use IConnectionMultiplexer.GetServer().Keys() + DeleteAsync().
        // For now, we log a warning and rely on TTL expiration.
        _logger.LogDebug(
            "Cache prefix removal requested for {Prefix}. Using TTL-based expiration instead.",
            prefix);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds a tenant-scoped cache key to prevent cross-tenant data leaks.
    /// Format: "LoyaltySphere:{tenantId}:{key}"
    /// </summary>
    private string BuildKey(string key)
    {
        var tenantId = _tenantContext.HasTenant ? _tenantContext.TenantId : "global";
        return $"tenant:{tenantId}:{key}";
    }
}
