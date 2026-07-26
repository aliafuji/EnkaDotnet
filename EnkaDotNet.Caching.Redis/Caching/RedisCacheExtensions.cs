using System;
using EnkaDotNet.Caching.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#nullable enable

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Opt in registration helpers for the Redis cache provider.
    /// </summary>
    public static class RedisCacheExtensions
    {
        /// <summary>
        /// Configures the client to use the Redis cache provider.
        /// </summary>
        /// <param name="options">The client options to configure.</param>
        /// <param name="configure">Optional callback for tweaking the Redis cache options.</param>
        /// <returns>The same options instance, for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        public static EnkaClientOptions UseRedisCache(this EnkaClientOptions options, Action<RedisCacheOptions>? configure = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.CacheProvider = CacheProvider.Redis;

            if (options.RedisCache == null)
            {
                options.RedisCache = new RedisCacheOptions();
            }

            configure?.Invoke(options.RedisCache);

            options.CacheFactoryOverride = clientOptions =>
                new RedisCacheProvider(clientOptions.RedisCache ?? new RedisCacheOptions());

            return options;
        }

        /// <summary>
        /// Registers the Redis cache provider. May be called before or after
        /// <c>AddEnkaNetClient</c>; the order does not matter. Calling this after
        /// <c>AddEnkaSqliteCache</c> replaces SQLite as the provider.
        /// </summary>
        /// <param name="services">The service collection to add the cache to.</param>
        /// <param name="configure">Optional callback for tweaking the Redis cache options.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public static IServiceCollection AddEnkaRedisCache(this IServiceCollection services, Action<RedisCacheOptions>? configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Replace(ServiceDescriptor.Singleton<IEnkaCacheProviderFactory>(
                new RedisCacheProviderFactory(configure)));

            return services;
        }

        private sealed class RedisCacheProviderFactory : IEnkaCacheProviderFactory
        {
            private readonly Action<RedisCacheOptions>? _configure;

            internal RedisCacheProviderFactory(Action<RedisCacheOptions>? configure)
            {
                _configure = configure;
            }

            public IEnkaCache CreateCache(EnkaClientOptions options)
            {
                // Routed through UseRedisCache so the DI path and the manual path share one
                // implementation, including the default TTL fallback.
                options.UseRedisCache(_configure);
                return CacheFactory.CreateCache(options);
            }
        }
    }
}
