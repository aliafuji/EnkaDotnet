using System;
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
    /// Property-based tests for MemoryCacheAdapter.
    /// Feature: persistent-cache, Property 1: Cache Round-Trip Consistency
    /// </summary>
    public class MemoryCacheAdapterPropertyTests
    {
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
                    return Name == other.Name && Value == other.Value && Timestamp == other.Timestamp;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, Value, Timestamp);
            }
        }

        /// <summary>
        /// Property 1: Cache Round-Trip Consistency
        /// For any valid cache key and serializable value, storing the value and then 
        /// retrieving it should produce an equivalent object.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property RoundTrip_StoreAndRetrieve_ProducesEquivalentObject()
        {
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<NonEmptyString>(),
                Arb.From<int>(),
                (key, name, value) =>
                {
                    // Arrange
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var adapter = new MemoryCacheAdapter(memoryCache);

                    var cacheKey = key.Get;
                    var originalData = new TestCacheData
                    {
                        Name = name.Get,
                        Value = value,
                        Timestamp = DateTime.UtcNow
                    };

                    // Act
                    adapter.SetAsync(cacheKey, originalData).GetAwaiter().GetResult();
                    var retrievedData = adapter.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();

                    // Assert
                    return (retrievedData != null &&
                            retrievedData.Name == originalData.Name &&
                            retrievedData.Value == originalData.Value)
                        .Label($"Round-trip failed for key '{cacheKey}' with name '{originalData.Name}' and value {originalData.Value}");
                });
        }

        /// <summary>
        /// Property: Statistics accuracy - hit count + miss count equals total Get operations.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property Statistics_HitPlusMiss_EqualsTotalGetOperations()
        {
            return Prop.ForAll(
                Arb.From<NonEmptyString>(),
                Arb.From<PositiveInt>(),
                (key, getCount) =>
                {
                    // Arrange
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var adapter = new MemoryCacheAdapter(memoryCache);

                    var cacheKey = key.Get;
                    var testData = new TestCacheData { Name = "Test", Value = 42, Timestamp = DateTime.UtcNow };
                    var totalGets = Math.Min(getCount.Get, 50); // Limit to reasonable number

                    // Act - perform some sets and gets
                    adapter.SetAsync(cacheKey, testData).GetAwaiter().GetResult();

                    for (int i = 0; i < totalGets; i++)
                    {
                        // Alternate between existing and non-existing keys
                        if (i % 2 == 0)
                        {
                            adapter.GetAsync<TestCacheData>(cacheKey).GetAwaiter().GetResult();
                        }
                        else
                        {
                            adapter.GetAsync<TestCacheData>($"nonexistent_{i}").GetAwaiter().GetResult();
                        }
                    }

                    var stats = adapter.GetStatsAsync().GetAwaiter().GetResult();

                    // Assert
                    return (stats.HitCount + stats.MissCount == totalGets)
                        .Label($"Expected {totalGets} total operations, got {stats.HitCount} hits + {stats.MissCount} misses = {stats.HitCount + stats.MissCount}");
                });
        }

        /// <summary>
        /// Property: Clear operation completeness - after clear, all keys should not exist.
        /// </summary>
        [Property(MaxTest = 100)]
        public Property Clear_AfterClear_AllKeysNotExist()
        {
            return Prop.ForAll(
                Arb.From<PositiveInt>(),
                (entryCount) =>
                {
                    // Arrange
                    using var memoryCache = new MemoryCache(new MemoryCacheOptions());
                    using var adapter = new MemoryCacheAdapter(memoryCache);

                    var count = Math.Min(entryCount.Get, 20); // Limit to reasonable number
                    var keys = new string[count];

                    // Add entries
                    for (int i = 0; i < count; i++)
                    {
                        keys[i] = $"key_{i}";
                        var testData = new TestCacheData { Name = $"Name_{i}", Value = i, Timestamp = DateTime.UtcNow };
                        adapter.SetAsync(keys[i], testData).GetAwaiter().GetResult();
                    }

                    // Act
                    adapter.ClearAsync().GetAwaiter().GetResult();

                    // Assert - all keys should not exist
                    bool allCleared = true;
                    foreach (var key in keys)
                    {
                        if (adapter.ExistsAsync(key).GetAwaiter().GetResult())
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
