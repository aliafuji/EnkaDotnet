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
        /// <exception cref="InvalidOperationException">
        /// Thrown when Custom provider is selected but no custom cache is provided, or when the
        /// SQLite or Redis provider is selected without the matching opt in package.
        /// </exception>
        public static IEnkaCache CreateCache(
            EnkaClientOptions options,
            IMemoryCache? memoryCache = null,
            IEnkaCache? customCache = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Normalised before the override runs: the opt in packages read these options and
            // cannot see the internal ExplicitDefaultTtl fallback rules themselves.
            if (options.SQLiteCache != null)
            {
                WithFallbackTtl(options.SQLiteCache, options);
            }

            if (options.RedisCache != null)
            {
                WithFallbackTtl(options.RedisCache, options);
            }

            if (options.CacheFactoryOverride != null)
            {
                return options.CacheFactoryOverride(options);
            }

            return options.CacheProvider switch
            {
                CacheProvider.Memory => CreateMemoryCache(options, memoryCache),
                CacheProvider.SQLite => throw MissingProviderPackage(
                    nameof(CacheProvider.SQLite), "EnkaDotNet.Caching.Sqlite", "UseSqliteCache", "AddEnkaSqliteCache"),
                CacheProvider.Redis => throw MissingProviderPackage(
                    nameof(CacheProvider.Redis), "EnkaDotNet.Caching.Redis", "UseRedisCache", "AddEnkaRedisCache"),
                CacheProvider.Custom => CreateCustomCache(customCache),
                _ => CreateMemoryCache(options, memoryCache)
            };
        }

        private static InvalidOperationException MissingProviderPackage(
            string provider, string packageId, string optionsMethod, string serviceMethod)
        {
            return new InvalidOperationException(
                $"CacheProvider.{provider} requires the {packageId} package. " +
                $"Install it and call options.{optionsMethod}(...) or services.{serviceMethod}(...).");
        }

        /// <summary>
        /// Default size limit, in serialized characters, applied to memory caches this factory
        /// creates. Without a limit the cache grows until the process runs out of memory.
        /// </summary>
        private const long DefaultMemoryCacheSizeLimit = 64L * 1024 * 1024;

        /// <summary>
        /// Creates a memory cache adapter.
        /// </summary>
        private static IEnkaCache CreateMemoryCache(EnkaClientOptions options, IMemoryCache? memoryCache)
        {
            var defaultTtl = TimeSpan.FromMinutes(options.CacheDurationMinutes);

            if (memoryCache != null)
            {
                // Caller supplied cache: it may be shared with the rest of the application, so the
                // adapter must not dispose it or assume anything about its size limit.
                return new MemoryCacheAdapter(memoryCache, defaultTtl);
            }

            var ownedCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = DefaultMemoryCacheSizeLimit });
            return new MemoryCacheAdapter(ownedCache, defaultTtl, jsonOptions: null, ownsMemoryCache: true);
        }

        private static SQLiteCacheOptions WithFallbackTtl(SQLiteCacheOptions cacheOptions, EnkaClientOptions options)
        {
            cacheOptions.DefaultTtl = cacheOptions.ExplicitDefaultTtl
                ?? TimeSpan.FromMinutes(options.CacheDurationMinutes);
            return cacheOptions;
        }

        private static RedisCacheOptions WithFallbackTtl(RedisCacheOptions cacheOptions, EnkaClientOptions options)
        {
            cacheOptions.DefaultTtl = cacheOptions.ExplicitDefaultTtl
                ?? TimeSpan.FromMinutes(options.CacheDurationMinutes);
            return cacheOptions;
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
