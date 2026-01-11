using System;
using System.IO;
using System.Threading.Tasks;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    /// <summary>
    /// Property-based tests for SQLiteCacheProvider.
    /// Feature: persistent-cache
    /// </summary>
    public class SQLiteCacheProviderPropertyTests : IDisposable
    {
        private readonly string _testDbDirectory;

        public SQLiteCacheProviderPropertyTests()
        {
            _testDbDirectory = Path.Combine(Path.GetTempPath(), $"EnkaTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDbDirectory);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDbDirectory))
                {
                    Directory.Delete(_testDbDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
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
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<NonEmptyString>(),
                Arb.From<int>(),
                (key, name, value) =>
                {
                    var dbPath = Path.Combine(_testDbDirectory, $"test_{Guid.NewGuid():N}.db");
                    var options = new SQLiteCacheOptions
                    {
                        DatabasePath = dbPath,
                        DefaultTtl = TimeSpan.FromMinutes(5),
                        EnableAutoCleanup = false
                    };

                    using var provider = new SQLiteCacheProvider(options);

                    var cacheKey = key.Get;
                    var originalData = new TestCacheData
                    {
                        Name = name.Get,
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    provider.SetAsync(cacheKey, originalData).GetAwaiter().GetResult();
                    var retrievedData = provider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();

                    return (retrievedData != null &&
                            retrievedData.Name == originalData.Name &&
                            retrievedData.Value == originalData.Value)
                        .Label($"Round-trip failed for key '{cacheKey}'");
                });
        }

        /// <summary>
        /// Property 2: Expiration Enforcement
        /// For any cache entry with a TTL, after the TTL has elapsed, retrieving the entry should return null.
        /// </summary>
        [Fact]
        public async Task Expiration_AfterTtlElapsed_ReturnsNull()
        {
            var dbPath = Path.Combine(_testDbDirectory, $"test_{Guid.NewGuid():N}.db");
            var options = new SQLiteCacheOptions
            {
                DatabasePath = dbPath,
                DefaultTtl = TimeSpan.FromMilliseconds(50),
                EnableAutoCleanup = false
            };

            using var provider = new SQLiteCacheProvider(options);

            var cacheKey = "test_key";
            var originalData = new TestCacheData
            {
                Name = "TestName",
                Value = 42,
                Timestamp = DateTime.UtcNow
            };

            await provider.SetAsync(cacheKey, originalData, TimeSpan.FromMilliseconds(50));
            await Task.Delay(100);
            var retrievedData = await provider.GetAsync<TestCacheData>(cacheKey);

            Assert.Null(retrievedData);
        }

        /// <summary>
        /// Property: Statistics accuracy - hit count + miss count equals total Get operations.
        /// </summary>
        [Property(MaxTest = 5)]
        public Property Statistics_HitPlusMiss_EqualsTotalGetOperations()
        {
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<PositiveInt>(),
                (key, getCount) =>
                {
                    var dbPath = Path.Combine(_testDbDirectory, $"test_{Guid.NewGuid():N}.db");
                    var options = new SQLiteCacheOptions
                    {
                        DatabasePath = dbPath,
                        DefaultTtl = TimeSpan.FromMinutes(5),
                        EnableAutoCleanup = false
                    };

                    using var provider = new SQLiteCacheProvider(options);

                    var cacheKey = key.Get;
                    var testData = new TestCacheData { Name = "Test", Value = 42, Timestamp = DateTime.UtcNow };
                    var totalGets = Math.Min(getCount.Get, 10);

                    provider.SetAsync(cacheKey, testData).GetAwaiter().GetResult();

                    for (int i = 0; i < totalGets; i++)
                    {
                        if (i % 2 == 0)
                            provider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();
                        else
                            provider.GetAsync<TestCacheData>($"nonexistent_{i}").GetAwaiter().GetResult();
                    }

                    var stats = provider.GetStatsAsync().GetAwaiter().GetResult();

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
            return Prop.ForAll(
                Arb.From<PositiveInt>(),
                (entryCount) =>
                {
                    var dbPath = Path.Combine(_testDbDirectory, $"test_{Guid.NewGuid():N}.db");
                    var options = new SQLiteCacheOptions
                    {
                        DatabasePath = dbPath,
                        DefaultTtl = TimeSpan.FromMinutes(5),
                        EnableAutoCleanup = false
                    };

                    using var provider = new SQLiteCacheProvider(options);

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
    }
}
