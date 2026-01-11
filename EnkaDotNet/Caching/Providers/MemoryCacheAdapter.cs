using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

#nullable enable

namespace EnkaDotNet.Caching.Providers
{
    /// <summary>
    /// Adapter that wraps IMemoryCache to implement IEnkaCache interface.
    /// Provides backward compatibility with existing memory cache functionality.
    /// </summary>
    public class MemoryCacheAdapter : IEnkaCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, bool> _trackedKeys;
        private readonly TimeSpan _defaultTtl;
        private readonly JsonSerializerOptions? _jsonOptions;
        private long _hitCount;
        private long _missCount;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the MemoryCacheAdapter class.
        /// </summary>
        /// <param name="memoryCache">The underlying IMemoryCache instance to wrap.</param>
        /// <param name="defaultTtl">Default time-to-live for cache entries. Defaults to 5 minutes.</param>
        /// <param name="jsonOptions">Optional JSON serializer options. If null, uses EnkaJsonContext for AOT compatibility on .NET 8+.</param>
        public MemoryCacheAdapter(IMemoryCache memoryCache, TimeSpan? defaultTtl = null, JsonSerializerOptions? jsonOptions = null)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _trackedKeys = new ConcurrentDictionary<string, bool>();
            _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(5);
            _jsonOptions = jsonOptions;
            _hitCount = 0;
            _missCount = 0;
            _disposed = false;
        }

        /// <summary>
        /// Gets the effective JSON serializer options, using EnkaJsonContext for AOT compatibility when available.
        /// </summary>
        private JsonSerializerOptions? GetEffectiveJsonOptions()
        {
            if (_jsonOptions != null)
            {
                return _jsonOptions;
            }
#if NET8_0_OR_GREATER
            return EnkaJsonContext.Default.Options;
#else
            return null;
#endif
        }

        /// <inheritdoc/>
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                Interlocked.Increment(ref _missCount);
                return Task.FromResult<T?>(null);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_memoryCache.TryGetValue(key, out MemoryCacheEntry? entry) && entry != null && !entry.IsExpired)
            {
                Interlocked.Increment(ref _hitCount);
                try
                {
                    var options = GetEffectiveJsonOptions();
#pragma warning disable IL2026, IL3050
                    var result = JsonSerializer.Deserialize<T>(entry.JsonValue, options);
#pragma warning restore IL2026, IL3050
                    return Task.FromResult(result);
                }
                catch (JsonException)
                {
                    // If deserialization fails, treat as miss
                    Interlocked.Decrement(ref _hitCount);
                    Interlocked.Increment(ref _missCount);
                    return Task.FromResult<T?>(null);
                }
                catch (NotSupportedException)
                {
                    // Type not in EnkaJsonContext, try without AOT options
                    try
                    {
#pragma warning disable IL2026, IL3050
                        var result = JsonSerializer.Deserialize<T>(entry.JsonValue);
#pragma warning restore IL2026, IL3050
                        return Task.FromResult(result);
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _hitCount);
                        Interlocked.Increment(ref _missCount);
                        return Task.FromResult<T?>(null);
                    }
                }
            }

            Interlocked.Increment(ref _missCount);
            return Task.FromResult<T?>(null);
        }

        /// <inheritdoc/>
        public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default) where T : class
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var effectiveTtl = ttl ?? _defaultTtl;
            var expiration = DateTime.UtcNow.Add(effectiveTtl);

            string jsonValue;
            var options = GetEffectiveJsonOptions();
            try
            {
#pragma warning disable IL2026, IL3050
                jsonValue = JsonSerializer.Serialize(value, options);
#pragma warning restore IL2026, IL3050
            }
            catch (NotSupportedException)
            {
                // Type not in EnkaJsonContext, try without AOT options
#pragma warning disable IL2026, IL3050
                jsonValue = JsonSerializer.Serialize(value);
#pragma warning restore IL2026, IL3050
            }

            var entry = new MemoryCacheEntry
            {
                JsonValue = jsonValue,
                Expiration = expiration
            };

            var entryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(effectiveTtl);

            _memoryCache.Set(key, entry, entryOptions);
            _trackedKeys.TryAdd(key, true);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                return Task.FromResult(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            bool existed = _trackedKeys.TryRemove(key, out _);
            _memoryCache.Remove(key);

            return Task.FromResult(existed);
        }

        /// <inheritdoc/>
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var key in _trackedKeys.Keys)
            {
                _memoryCache.Remove(key);
            }
            _trackedKeys.Clear();

            // If the underlying cache is a concrete MemoryCache, compact it
            if (_memoryCache is MemoryCache concreteCache)
            {
                concreteCache.Compact(1.0);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                return Task.FromResult(false);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_memoryCache.TryGetValue(key, out MemoryCacheEntry? entry) && entry != null && !entry.IsExpired)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<CacheStatistics> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            long entryCount = 0;
            if (_memoryCache is MemoryCache concreteCache)
            {
                entryCount = concreteCache.Count;
            }
            else
            {
                // Fallback to tracked keys count
                entryCount = _trackedKeys.Count;
            }

            var stats = new CacheStatistics
            {
                HitCount = Interlocked.Read(ref _hitCount),
                MissCount = Interlocked.Read(ref _missCount),
                EntryCount = entryCount,
                SizeBytes = null // Memory cache doesn't provide size information
            };

            return Task.FromResult(stats);
        }

        /// <summary>
        /// Throws ObjectDisposedException if the adapter has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MemoryCacheAdapter));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MemoryCacheAdapter.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _trackedKeys.Clear();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Internal cache entry for storing serialized values with expiration.
        /// </summary>
        internal class MemoryCacheEntry
        {
            public string JsonValue { get; set; } = string.Empty;
            public DateTime Expiration { get; set; }
            public bool IsExpired => DateTime.UtcNow > Expiration;
        }
    }
}
