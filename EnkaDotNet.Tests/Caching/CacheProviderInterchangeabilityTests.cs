using System;
using System.IO;
using System.Threading.Tasks;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace EnkaDotNet.Tests.Caching
{
    /// <summary>
    /// Property-based tests for cache provider interchangeability.
    /// Feature: persistent-cache, Property 3: Provider Interchangeability
    /// </summary>
    public class CacheProviderInterchangeabilityTests : IDisposable
    {
        private readonly string _testDbPath;

        public CacheProviderInterchangeabilityTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"enka_test_interop_{Guid.NewGuid()}.db");
        }

        public void Dispose()
        {
            // Cleanup test database
            try
            {
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Test data class for cache interchangeability testing.
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
        /// Property 3: Provider Interchangeability
        /// For any sequence of cache operations, switching between cache providers 
        /// (Memory, SQLite) should produce the same observable behavior (ignoring persistence).
        /// 
        /// This test verifies that both Memory and SQLite providers behave identically
        /// for the same sequence of operations.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property ProviderInterchangeability_SameOperations_SameBehavior()
        {
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<NonEmptyString>(),
                Arb.From<int>(),
                (key, name, value) =>
                {
                    var cacheKey = key.Get;
                    var testData = new TestCacheData
                    {
                        Name = name.Get,
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    // Test with Memory provider
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var memoryAdapter = new MemoryCacheAdapter(memoryCache);

                    // Test with SQLite provider
                    var dbPath = Path.Combine(Path.GetTempPath(), $"enka_test_{Guid.NewGuid()}.db");
                    try
                    {
                        using var sqliteProvider = new SQLiteCacheProvider(new SQLiteCacheOptions
                        {
                            DatabasePath = dbPath,
                            EnableAutoCleanup = false
                        });

                        // Operation 1: Set value
                        memoryAdapter.SetAsync(cacheKey, testData).GetAwaiter().GetResult();
                        sqliteProvider.SetAsync(cacheKey, testData).GetAwaiter().GetResult();

                        // Operation 2: Check exists
                        var memoryExists = memoryAdapter.ExistsAsync(cacheKey).GetAwaiter().GetResult();
                        var sqliteExists = sqliteProvider.ExistsAsync(cacheKey).GetAwaiter().GetResult();

                        if (memoryExists != sqliteExists)
                        {
                            return false.Label($"ExistsAsync mismatch: Memory={memoryExists}, SQLite={sqliteExists}");
                        }

                        // Operation 3: Get value
                        var memoryResult = memoryAdapter.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();
                        var sqliteResult = sqliteProvider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();

                        if (memoryResult == null || sqliteResult == null)
                        {
                            return false.Label($"GetAsync returned null: Memory={memoryResult != null}, SQLite={sqliteResult != null}");
                        }

                        if (!memoryResult.Equals(sqliteResult))
                        {
                            return false.Label($"GetAsync value mismatch: Memory={memoryResult.Name}/{memoryResult.Value}, SQLite={sqliteResult.Name}/{sqliteResult.Value}");
                        }

                        // Operation 4: Remove value
                        var memoryRemoved = memoryAdapter.RemoveAsync(cacheKey).GetAwaiter().GetResult();
                        var sqliteRemoved = sqliteProvider.RemoveAsync(cacheKey).GetAwaiter().GetResult();

                        if (memoryRemoved != sqliteRemoved)
                        {
                            return false.Label($"RemoveAsync mismatch: Memory={memoryRemoved}, SQLite={sqliteRemoved}");
                        }

                        // Operation 5: Check not exists after remove
                        var memoryExistsAfter = memoryAdapter.ExistsAsync(cacheKey).GetAwaiter().GetResult();
                        var sqliteExistsAfter = sqliteProvider.ExistsAsync(cacheKey).GetAwaiter().GetResult();

                        if (memoryExistsAfter != sqliteExistsAfter)
                        {
                            return false.Label($"ExistsAsync after remove mismatch: Memory={memoryExistsAfter}, SQLite={sqliteExistsAfter}");
                        }

                        return true.Label("All operations produced same behavior");
                    }
                    finally
                    {
                        // Cleanup
                        try
                        {
                            if (File.Exists(dbPath))
                            {
                                File.Delete(dbPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                });
        }

        /// <summary>
        /// Property: Clear operation behaves the same across providers.
        /// </summary>
        [Property(MaxTest = 50)]
        public Property ProviderInterchangeability_ClearOperation_SameBehavior()
        {
            return Prop.ForAll(
                Arb.From<PositiveInt>(),
                (entryCount) =>
                {
                    var count = Math.Min(entryCount.Get, 10); // Limit for performance

                    // Test with Memory provider
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var memoryAdapter = new MemoryCacheAdapter(memoryCache);

                    // Test with SQLite provider
                    var dbPath = Path.Combine(Path.GetTempPath(), $"enka_test_clear_{Guid.NewGuid()}.db");
                    try
                    {
                        using var sqliteProvider = new SQLiteCacheProvider(new SQLiteCacheOptions
                        {
                            DatabasePath = dbPath,
                            EnableAutoCleanup = false
                        });

                        // Add entries to both providers
                        for (int i = 0; i < count; i++)
                        {
                            var key = $"key_{i}";
                            var data = new TestCacheData { Name = $"Name_{i}", Value = i, Timestamp = DateTime.UtcNow };
                            memoryAdapter.SetAsync(key, data).GetAwaiter().GetResult();
                            sqliteProvider.SetAsync(key, data).GetAwaiter().GetResult();
                        }

                        // Clear both providers
                        memoryAdapter.ClearAsync().GetAwaiter().GetResult();
                        sqliteProvider.ClearAsync().GetAwaiter().GetResult();

                        // Verify all keys are cleared in both
                        for (int i = 0; i < count; i++)
                        {
                            var key = $"key_{i}";
                            var memoryExists = memoryAdapter.ExistsAsync(key).GetAwaiter().GetResult();
                            var sqliteExists = sqliteProvider.ExistsAsync(key).GetAwaiter().GetResult();

                            if (memoryExists || sqliteExists)
                            {
                                return false.Label($"Key {key} still exists after clear: Memory={memoryExists}, SQLite={sqliteExists}");
                            }
                        }

                        return true.Label("Clear operation behaved the same across providers");
                    }
                    finally
                    {
                        // Cleanup
                        try
                        {
                            if (File.Exists(dbPath))
                            {
                                File.Delete(dbPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                });
        }

        /// <summary>
        /// Property: Get on non-existent key returns null for all providers.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property ProviderInterchangeability_GetNonExistent_ReturnsNull()
        {
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                (key) =>
                {
                    var cacheKey = key.Get;

                    // Test with Memory provider
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var memoryAdapter = new MemoryCacheAdapter(memoryCache);

                    // Test with SQLite provider
                    var dbPath = Path.Combine(Path.GetTempPath(), $"enka_test_null_{Guid.NewGuid()}.db");
                    try
                    {
                        using var sqliteProvider = new SQLiteCacheProvider(new SQLiteCacheOptions
                        {
                            DatabasePath = dbPath,
                            EnableAutoCleanup = false
                        });

                        // Get non-existent key from both
                        var memoryResult = memoryAdapter.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();
                        var sqliteResult = sqliteProvider.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();

                        // Both should return null
                        return (memoryResult == null && sqliteResult == null)
                            .Label($"Non-existent key should return null: Memory={memoryResult == null}, SQLite={sqliteResult == null}");
                    }
                    finally
                    {
                        // Cleanup
                        try
                        {
                            if (File.Exists(dbPath))
                            {
                                File.Delete(dbPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }
                    }
                });
        }
    }
}
