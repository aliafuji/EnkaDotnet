using System;
using System.IO;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using EnkaDotNet.DIExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    public class CacheRegistrationTests
    {
        [Fact]
        public void AddEnkaNetClient_DefaultOptions_RegistersMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddEnkaNetClient();

            using var provider = services.BuildServiceProvider();

            Assert.IsType<MemoryCacheAdapter>(provider.GetRequiredService<IEnkaCache>());
        }

        [Fact]
        public void AddEnkaSqliteCache_SQLiteProvider_RegistersSQLiteCache()
        {
            string databasePath = Path.Combine(Path.GetTempPath(), "enka-di-" + Guid.NewGuid().ToString("N") + ".db");
            var services = new ServiceCollection();
            services.AddEnkaSqliteCache();
            services.AddEnkaNetClient(options =>
            {
                options.CacheProvider = CacheProvider.SQLite;
                options.SQLiteCache = new SQLiteCacheOptions
                {
                    DatabasePath = databasePath,
                    EnableAutoCleanup = false
                };
            });

            try
            {
                using var provider = services.BuildServiceProvider();
                Assert.IsType<SQLiteCacheProvider>(provider.GetRequiredService<IEnkaCache>());
            }
            finally
            {
                SqliteTestCleanup.TryDelete(databasePath);
            }
        }

        [Fact]
        public void AddEnkaSqliteCache_RegisteredAfterClient_StillWins()
        {
            string databasePath = Path.Combine(Path.GetTempPath(), "enka-order-" + Guid.NewGuid().ToString("N") + ".db");
            var services = new ServiceCollection();
            services.AddEnkaNetClient(options =>
            {
                options.CacheProvider = CacheProvider.SQLite;
                options.SQLiteCache = new SQLiteCacheOptions
                {
                    DatabasePath = databasePath,
                    EnableAutoCleanup = false
                };
            });
            services.AddEnkaSqliteCache();

            try
            {
                using var provider = services.BuildServiceProvider();
                Assert.IsType<SQLiteCacheProvider>(provider.GetRequiredService<IEnkaCache>());
            }
            finally
            {
                SqliteTestCleanup.TryDelete(databasePath);
            }
        }

        [Fact]
        public void AddEnkaNetClient_SQLiteProviderWithoutPackageHook_ThrowsActionableError()
        {
            var services = new ServiceCollection();
            services.AddEnkaNetClient(options => options.CacheProvider = CacheProvider.SQLite);

            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IEnkaCache>());
            Assert.Equal(
                "CacheProvider.SQLite requires the EnkaDotNet.Caching.Sqlite package. " +
                "Install it and call options.UseSqliteCache(...) or services.AddEnkaSqliteCache(...).",
                exception.Message);
        }

        [Fact]
        public void AddEnkaNetClient_RedisProviderWithoutPackageHook_ThrowsActionableError()
        {
            var services = new ServiceCollection();
            services.AddEnkaNetClient(options => options.CacheProvider = CacheProvider.Redis);

            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IEnkaCache>());
            Assert.Equal(
                "CacheProvider.Redis requires the EnkaDotNet.Caching.Redis package. " +
                "Install it and call options.UseRedisCache(...) or services.AddEnkaRedisCache(...).",
                exception.Message);
        }

        [Fact]
        public void UseSqliteCache_OnOptions_CreatesSQLiteCache()
        {
            string databasePath = Path.Combine(Path.GetTempPath(), "enka-opt-" + Guid.NewGuid().ToString("N") + ".db");
            var options = new EnkaClientOptions().UseSqliteCache(sqlite =>
            {
                sqlite.DatabasePath = databasePath;
                sqlite.EnableAutoCleanup = false;
            });

            try
            {
                Assert.Equal(CacheProvider.SQLite, options.CacheProvider);
                using var cache = CacheFactory.CreateCache(options);
                Assert.IsType<SQLiteCacheProvider>(cache);
            }
            finally
            {
                SqliteTestCleanup.TryDelete(databasePath);
            }
        }

        [Fact]
        public void AddEnkaNetClient_CustomCacheRegisteredFirst_IsPreserved()
        {
            var services = new ServiceCollection();
            var custom = new MemoryCacheAdapter(
                new Microsoft.Extensions.Caching.Memory.MemoryCache(
                    new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()),
                TimeSpan.FromMinutes(1));

            services.AddSingleton<IEnkaCache>(custom);
            services.AddEnkaNetClient(options => options.CacheProvider = CacheProvider.Custom);

            using var provider = services.BuildServiceProvider();

            Assert.Same(custom, provider.GetRequiredService<IEnkaCache>());
        }

        private static class SqliteTestCleanup
        {
            public static void TryDelete(string path)
            {
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                foreach (var candidate in new[] { path, path + "-wal", path + "-shm" })
                {
                    try { if (File.Exists(candidate)) File.Delete(candidate); }
                    catch (IOException) { }
                }
            }
        }
    }
}
