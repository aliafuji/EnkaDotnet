using System;
using System.Threading.Tasks;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using FsCheck;
using FsCheck.Xunit;
using StackExchange.Redis;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    /// <summary>
    /// Property-based tests for RedisCacheProvider.
    /// These tests require a running Redis server at localhost:6379.
    /// Tests will be skipped if Redis is not available.
    /// </summary>
    [Collection("Redis")]
    public class RedisCacheProviderPropertyTests : IDisposable
    {
        private readonly string _keyPrefix;
        private readonly RedisCacheOptions _options;
        private readonly bool _redisAvailable;
        private IConnectionMultiplexer? _redis;

        public RedisCacheProviderPropertyTests()
        {
            _keyPrefix = $"enka_test_{Guid.NewGuid():N}:";
            _options = new RedisCacheOptions
            {
                ConnectionString = "localhost:6379",
                KeyPrefix = _keyPrefix,
                DefaultTtl = TimeSpan.FromMinutes(5),
                ConnectTimeout = TimeSpan.FromSeconds(2),
                ConnectRetry = 1
            };

            _redisAvailable = TryConnectToRedis();
        }

        private bool TryConnectToRedis()
        {
            try
            {
                var configOptions = ConfigurationOptions.Parse(_options.ConnectionString);
                configOptions.ConnectTimeout = (int)_options.ConnectTimeout.TotalMilliseconds;
                configOptions.ConnectRetry = _options.ConnectRetry;
                configOptions.AbortOnConnectFail = true;

                _redis = ConnectionMultiplexer.Connect(configOptions);
                return _redis.IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            // Clean up test keys
            if (_redis != null && _redis.IsConnected)
            {
                try
                {
                    var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                    var keys = server.Keys(pattern: $"{_keyPrefix}*");
                    var db = _redis.GetDatabase();
                    foreach (var key in keys)
                    {
                        db.KeyDelete(key);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
                finally
                {
                    _redis.Dispose();
                }
            }
        }

        /// <summary>
        /// Test data class for cache round-trip testing.
        /// </summary>
        public class TestCacheData
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
            public DateTime Timestamp { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is TestCacheData other)
                {
                    return Name == other.Name && Value == other.Value;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, Value);
            }
        }

        /// <summary>
        /// Property 1: Cache Round-Trip Consistency
        /// For any valid cache key and serializable value, storing the value and then 
        /// retrieving it should produce an equivalent object.
        /// </summary>
        [Property(MaxTest = 5)]
        public Property RoundTrip_StoreAndRetrieve_ProducesEquivalentObject()
        {
            if (!_redisAvailable)
            {
                return true.ToProperty().Label("Skipped: Redis not available");
            }

            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<NonEmptyString>(),
                Arb.From<int>(),
                (key, name, value) =>
                {
                    var options = new RedisCacheOptions
                    {
                        ConnectionString = _options.ConnectionString,
                        KeyPrefix = _keyPrefix,
                        DefaultTtl = TimeSpan.FromMinutes(5)
                    };

                    using var provider = new RedisCacheProvider(_redis!, options);

                    var cacheKey = $"roundtrip_{Guid.NewGuid():N}_{key.Get}";
                    var originalData = new TestCacheData
                    {
                        Name = name.Get,
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    provider.SetAsync(cacheKey, originalData).GetAwaiter().GetResult();
                    var retrievedData = provider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();

                    // Cleanup
                    provider.RemoveAsync(cacheKey).GetAwaiter().GetResult();

                    return (retrievedData != null &&
                            retrievedData.Name == originalData.Name &&
                            retrievedData.Value == originalData.Value)
                        .Label($"Round-trip failed for key '{cacheKey}'");
                });
        }


        /// <summary>
        /// Property: Expiration Enforcement
        /// For any cache entry with a TTL, after the TTL has elapsed, retrieving the entry should return null.
        /// </summary>
        [Fact]
        public async Task Expiration_AfterTtlElapsed_ReturnsNull()
        {
            if (!_redisAvailable)
            {
                return; // Skip test if Redis not available
            }

            var options = new RedisCacheOptions
            {
                ConnectionString = _options.ConnectionString,
                KeyPrefix = _keyPrefix,
                DefaultTtl = TimeSpan.FromMilliseconds(100)
            };

            using var provider = new RedisCacheProvider(_redis!, options);

            var cacheKey = $"expiration_test_{Guid.NewGuid():N}";
            var originalData = new TestCacheData
            {
                Name = "TestName",
                Value = 42,
                Timestamp = DateTime.UtcNow
            };

            await provider.SetAsync(cacheKey, originalData, TimeSpan.FromMilliseconds(100));
            await Task.Delay(200);
            var retrievedData = await provider.GetAsync<TestCacheData>(cacheKey);

            Assert.Null(retrievedData);
        }

        /// <summary>
        /// Property: Statistics accuracy - hit count + miss count equals total Get operations.
        /// </summary>
        [Property(MaxTest = 5)]
        public Property Statistics_HitPlusMiss_EqualsTotalGetOperations()
        {
            if (!_redisAvailable)
            {
                return true.ToProperty().Label("Skipped: Redis not available");
            }

            return Prop.ForAll(
                Arb.From<PositiveInt>(),
                (getCount) =>
                {
                    var options = new RedisCacheOptions
                    {
                        ConnectionString = _options.ConnectionString,
                        KeyPrefix = $"{_keyPrefix}stats_{Guid.NewGuid():N}_",
                        DefaultTtl = TimeSpan.FromMinutes(5)
                    };

                    using var provider = new RedisCacheProvider(_redis!, options);

                    var cacheKey = $"stats_test_{Guid.NewGuid():N}";
                    var testData = new TestCacheData { Name = "Test", Value = 42, Timestamp = DateTime.UtcNow };
                    var totalGets = Math.Min(getCount.Get, 10);

                    provider.SetAsync(cacheKey, testData).GetAwaiter().GetResult();

                    for (int i = 0; i < totalGets; i++)
                    {
                        if (i % 2 == 0)
                            provider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();
                        else
                            provider.GetAsync<TestCacheData>($"nonexistent_{Guid.NewGuid():N}").GetAwaiter().GetResult();
                    }

                    var stats = provider.GetStatsAsync().GetAwaiter().GetResult();

                    // Cleanup
                    provider.RemoveAsync(cacheKey).GetAwaiter().GetResult();

                    return (stats.HitCount + stats.MissCount == totalGets)
                        .Label($"Expected {totalGets}, got {stats.HitCount + stats.MissCount}");
                });
        }

        /// <summary>
        /// Property: Clear operation completeness - after clear, all keys should not exist.
        /// </summary>
        [Property(MaxTest = 5)]
        public Property Clear_AfterClear_AllKeysNotExist()
        {
            if (!_redisAvailable)
            {
                return true.ToProperty().Label("Skipped: Redis not available");
            }

            return Prop.ForAll(
                Arb.From<PositiveInt>(),
                (entryCount) =>
                {
                    var uniquePrefix = $"{_keyPrefix}clear_{Guid.NewGuid():N}_";
                    var options = new RedisCacheOptions
                    {
                        ConnectionString = _options.ConnectionString,
                        KeyPrefix = uniquePrefix,
                        DefaultTtl = TimeSpan.FromMinutes(5)
                    };

                    using var provider = new RedisCacheProvider(_redis!, options);

                    var count = Math.Min(entryCount.Get, 5);
                    var keys = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        keys[i] = $"key_{i}";
                        var testData = new TestCacheData { Name = $"Name_{i}", Value = i, Timestamp = DateTime.UtcNow };
                        provider.SetAsync(keys[i], testData).GetAwaiter().GetResult();
                    }

                    provider.ClearAsync().GetAwaiter().GetResult();

                    bool allCleared = true;
                    foreach (var keyItem in keys)
                    {
                        if (provider.ExistsAsync(keyItem).GetAwaiter().GetResult())
                        {
                            allCleared = false;
                            break;
                        }
                    }

                    return allCleared.Label($"Not all {count} keys were cleared");
                });
        }

        /// <summary>
        /// Test: Key prefix isolation - keys with different prefixes should not interfere.
        /// </summary>
        [Fact]
        public async Task KeyPrefix_DifferentPrefixes_DoNotInterfere()
        {
            if (!_redisAvailable)
            {
                return; // Skip test if Redis not available
            }

            var prefix1 = $"{_keyPrefix}prefix1_";
            var prefix2 = $"{_keyPrefix}prefix2_";

            var options1 = new RedisCacheOptions
            {
                ConnectionString = _options.ConnectionString,
                KeyPrefix = prefix1,
                DefaultTtl = TimeSpan.FromMinutes(5)
            };

            var options2 = new RedisCacheOptions
            {
                ConnectionString = _options.ConnectionString,
                KeyPrefix = prefix2,
                DefaultTtl = TimeSpan.FromMinutes(5)
            };

            using var provider1 = new RedisCacheProvider(_redis!, options1);
            using var provider2 = new RedisCacheProvider(_redis!, options2);

            var cacheKey = "shared_key";
            var data1 = new TestCacheData { Name = "Provider1", Value = 1, Timestamp = DateTime.UtcNow };
            var data2 = new TestCacheData { Name = "Provider2", Value = 2, Timestamp = DateTime.UtcNow };

            await provider1.SetAsync(cacheKey, data1);
            await provider2.SetAsync(cacheKey, data2);

            var retrieved1 = await provider1.GetAsync<TestCacheData>(cacheKey);
            var retrieved2 = await provider2.GetAsync<TestCacheData>(cacheKey);

            // Cleanup
            await provider1.RemoveAsync(cacheKey);
            await provider2.RemoveAsync(cacheKey);

            Assert.NotNull(retrieved1);
            Assert.NotNull(retrieved2);
            Assert.Equal("Provider1", retrieved1.Name);
            Assert.Equal("Provider2", retrieved2.Name);
        }
    }
}
