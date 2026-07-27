using System;
using EnkaDotNet.Caching.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#nullable enable

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Opt in registration helpers for the SQLite cache provider.
    /// </summary>
    public static class SqliteCacheExtensions
    {
        /// <summary>
        /// Configures the client to use the SQLite cache provider.
        /// </summary>
        /// <param name="options">The client options to configure.</param>
        /// <param name="configure">Optional callback for tweaking the SQLite cache options.</param>
        /// <returns>The same options instance, for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
        public static EnkaClientOptions UseSqliteCache(this EnkaClientOptions options, Action<SQLiteCacheOptions>? configure = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.CacheProvider = CacheProvider.SQLite;

            if (options.SQLiteCache == null)
            {
                options.SQLiteCache = new SQLiteCacheOptions();
            }

            configure?.Invoke(options.SQLiteCache);

            options.CacheFactoryOverride = clientOptions =>
                new SQLiteCacheProvider(clientOptions.SQLiteCache ?? new SQLiteCacheOptions());

            return options;
        }

        /// <summary>
        /// Registers the SQLite cache provider. May be called before or after
        /// <c>AddEnkaNetClient</c>; the order does not matter. Calling this after
        /// <c>AddEnkaRedisCache</c> replaces Redis as the provider.
        /// </summary>
        /// <param name="services">The service collection to add the cache to.</param>
        /// <param name="configure">Optional callback for tweaking the SQLite cache options.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public static IServiceCollection AddEnkaSqliteCache(this IServiceCollection services, Action<SQLiteCacheOptions>? configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Replace(ServiceDescriptor.Singleton<IEnkaCacheProviderFactory>(
                new SqliteCacheProviderFactory(configure)));

            return services;
        }

        private sealed class SqliteCacheProviderFactory : IEnkaCacheProviderFactory
        {
            private readonly Action<SQLiteCacheOptions>? _configure;

            internal SqliteCacheProviderFactory(Action<SQLiteCacheOptions>? configure)
            {
                _configure = configure;
            }

            public IEnkaCache CreateCache(EnkaClientOptions options)
            {
                options.UseSqliteCache(_configure);
                return CacheFactory.CreateCache(options);
            }
        }
    }
}
