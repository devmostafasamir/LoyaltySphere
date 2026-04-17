namespace LoyaltySphere.RewardService.Application.Interfaces;

/// <summary>
/// Abstraction over distributed caching.
/// Provides tenant-aware, typed cache operations with graceful degradation.
/// Cache failures never crash the application — they degrade to a cache miss.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key. Returns null on cache miss or failure.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a value in cache with the specified expiration.
    /// Failures are logged but never thrown.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values matching a key pattern (e.g., "customer:*").
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
