using System;
using EnkaDotNet.Caching;
using EnkaDotNet.Exceptions;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    /// <summary>
    /// Unit tests for cache configuration validation.
    /// Feature: persistent-cache
    /// </summary>
    public class CacheConfigurationValidationTests
    {
        #region SQLiteCacheOptions Validation Tests

        [Fact]
        public void SQLiteOptions_NullDatabasePath_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = null!
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("DatabasePath", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_EmptyDatabasePath_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = ""
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("DatabasePath", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_WhitespaceDatabasePath_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "   "
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("DatabasePath", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_ZeroDefaultTtl_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "test.db",
                DefaultTtl = TimeSpan.Zero
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("DefaultTtl", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_NegativeDefaultTtl_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "test.db",
                DefaultTtl = TimeSpan.FromMinutes(-5)
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("DefaultTtl", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_ZeroCleanupIntervalWithAutoCleanup_ThrowsCacheException()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "test.db",
                EnableAutoCleanup = true,
                CleanupInterval = TimeSpan.Zero
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
            Assert.Equal("CleanupInterval", exception.ConfigurationProperty);
        }

        [Fact]
        public void SQLiteOptions_ZeroCleanupIntervalWithoutAutoCleanup_DoesNotThrow()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "test.db",
                EnableAutoCleanup = false,
                CleanupInterval = TimeSpan.Zero
            };

            // Should not throw because auto cleanup is disabled
            options.Validate();
        }

        [Fact]
        public void SQLiteOptions_ValidConfiguration_DoesNotThrow()
        {
            var options = new SQLiteCacheOptions
            {
                DatabasePath = "test.db",
                DefaultTtl = TimeSpan.FromMinutes(5),
                EnableAutoCleanup = true,
                CleanupInterval = TimeSpan.FromMinutes(30)
            };

            // Should not throw
            options.Validate();
        }

        #endregion

        #region RedisCacheOptions Validation Tests

        [Fact]
        public void RedisOptions_NullConnectionString_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = null!
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_EmptyConnectionString_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = ""
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_WhitespaceConnectionString_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "   "
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_InvalidPortNumber_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:99999"
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_NonNumericPort_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:abc"
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_ZeroDefaultTtl_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                DefaultTtl = TimeSpan.Zero
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("DefaultTtl", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_NegativeDefaultTtl_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                DefaultTtl = TimeSpan.FromMinutes(-5)
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("DefaultTtl", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_NegativeConnectRetry_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                ConnectRetry = -1
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectRetry", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_ZeroConnectTimeout_ThrowsCacheException()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                ConnectTimeout = TimeSpan.Zero
            };

            var exception = Assert.Throws<CacheException>(() => options.Validate());
            Assert.Equal(CacheProvider.Redis, exception.Provider);
            Assert.Equal("ConnectTimeout", exception.ConfigurationProperty);
        }

        [Fact]
        public void RedisOptions_ValidConfiguration_DoesNotThrow()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                KeyPrefix = "test:",
                DefaultTtl = TimeSpan.FromMinutes(5),
                ConnectRetry = 3,
                ConnectTimeout = TimeSpan.FromSeconds(5)
            };

            // Should not throw
            options.Validate();
        }

        [Fact]
        public void RedisOptions_ValidConfigurationWithOptions_DoesNotThrow()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379,password=secret,ssl=true",
                KeyPrefix = "test:",
                DefaultTtl = TimeSpan.FromMinutes(5)
            };

            // Should not throw
            options.Validate();
        }

        [Fact]
        public void RedisOptions_HostOnlyConnectionString_DoesNotThrow()
        {
            var options = new RedisCacheOptions
            {
                ConnectionString = "localhost"
            };

            // Should not throw - host only is valid
            options.Validate();
        }

        #endregion

        #region CacheException Tests

        [Fact]
        public void CacheException_WithProvider_ContainsProviderInMessage()
        {
            var exception = new CacheException(CacheProvider.SQLite, "Test error");

            Assert.Contains("SQLite", exception.Message);
            Assert.Equal(CacheProvider.SQLite, exception.Provider);
        }

        [Fact]
        public void CacheException_WithConfigurationProperty_ContainsPropertyInMessage()
        {
            var exception = new CacheException(CacheProvider.Redis, "Test error", "ConnectionString");

            Assert.Contains("ConnectionString", exception.Message);
            Assert.Contains("Redis", exception.Message);
            Assert.Equal("ConnectionString", exception.ConfigurationProperty);
        }

        [Fact]
        public void CacheException_WithInnerException_PreservesInnerException()
        {
            var innerException = new InvalidOperationException("Inner error");
            var exception = new CacheException(CacheProvider.SQLite, "Outer error", innerException);

            Assert.Equal(innerException, exception.InnerException);
        }

        #endregion
    }
}
