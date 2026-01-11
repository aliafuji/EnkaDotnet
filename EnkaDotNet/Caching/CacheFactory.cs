using System;
using EnkaDotNet.Caching.Providers;
using Microsoft.Extensions.Caching.Memory;

#nullable enable

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Factory for creating cache provider instances based on configuration.
    /// </summary>
    public static class CacheFactory
    {
        /// <summary>
        /// Creates a cache provider based on the specified options.
        /// </summary>
        /// <param name="options">The client options containing cache configuration.</param>
        /// <param name="memoryCache">Optional memory cache instance for Memory provider.</param>
        /// <param name="customCache">Optional custom cache instance for Custom provider.</param>
        /// <returns>An IEnkaCache implementation based on the configured provider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Custom provider is selected but no custom cache is provided.</exception>
        public static IEnkaCache CreateCache(
            EnkaClientOptions options,
            IMemoryCache? memoryCache = null,
            IEnkaCache? customCache = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return options.CacheProvider switch
            {
                CacheProvider.Memory => CreateMemoryCache(options, memoryCache),
                CacheProvider.SQLite => CreateSQLiteCache(options),
                CacheProvider.Redis => CreateRedisCache(options),
                CacheProvider.Custom => CreateCustomCache(customCache),
                _ => CreateMemoryCache(options, memoryCache)
            };
        }

        /// <summary>
        /// Creates a memory cache adapter.
        /// </summary>
        private static IEnkaCache CreateMemoryCache(EnkaClientOptions options, IMemoryCache? memoryCache)
        {
            var cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
            var defaultTtl = TimeSpan.FromMinutes(options.CacheDurationMinutes);
            return new MemoryCacheAdapter(cache, defaultTtl);
        }

        /// <summary>
        /// Creates a SQLite cache provider.
        /// </summary>
        private static IEnkaCache CreateSQLiteCache(EnkaClientOptions options)
        {
            var sqliteOptions = options.SQLiteCache ?? new SQLiteCacheOptions();
            
            // Override default TTL with client options if not explicitly set
            if (sqliteOptions.DefaultTtl == TimeSpan.FromMinutes(5) && options.CacheDurationMinutes != 5)
            {
                sqliteOptions.DefaultTtl = TimeSpan.FromMinutes(options.CacheDurationMinutes);
            }
            
            return new SQLiteCacheProvider(sqliteOptions);
        }

        /// <summary>
        /// Creates a Redis cache provider.
        /// </summary>
        private static IEnkaCache CreateRedisCache(EnkaClientOptions options)
        {
            var redisOptions = options.RedisCache ?? new RedisCacheOptions();
            
            // Override default TTL with client options if not explicitly set
            if (redisOptions.DefaultTtl == TimeSpan.FromMinutes(5) && options.CacheDurationMinutes != 5)
            {
                redisOptions.DefaultTtl = TimeSpan.FromMinutes(options.CacheDurationMinutes);
            }
            
            return new RedisCacheProvider(redisOptions);
        }

        /// <summary>
        /// Returns the custom cache provider.
        /// </summary>
        private static IEnkaCache CreateCustomCache(IEnkaCache? customCache)
        {
            if (customCache == null)
            {
                throw new InvalidOperationException(
                    "Custom cache provider was selected but no IEnkaCache instance was provided. " +
                    "Please provide a custom cache implementation via dependency injection or the customCache parameter.");
            }
            
            return customCache;
        }
    }
}
