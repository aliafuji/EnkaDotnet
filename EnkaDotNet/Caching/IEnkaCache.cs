using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Defines the contract for cache providers used by EnkaDotNet
    /// </summary>
    public interface IEnkaCache : IDisposable
    {
        /// <summary>
        /// Retrieves a cached value by key
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>The cached value, or null if not found or expired</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Stores a value in the cache
        /// </summary>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="ttl">Optional time-to-live for the cache entry</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Removes a cached value by key
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>True if the key was removed, false if it didn't exist</returns>
        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all entries from the cache
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a key exists in the cache
        /// </summary>
        /// <param name="key">The cache key to check</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>True if the key exists and is not expired, false otherwise</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets statistics about cache performance and usage
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Cache statistics including hit/miss counts and entry count</returns>
        Task<CacheStatistics> GetStatsAsync(CancellationToken cancellationToken = default);
    }
}
