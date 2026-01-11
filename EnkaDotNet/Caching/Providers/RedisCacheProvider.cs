using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using EnkaDotNet.Exceptions;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

#nullable enable

namespace EnkaDotNet.Caching.Providers
{
    /// <summary>
    /// Redis-based cache provider for distributed caching.
    /// Stores cache entries in a Redis server.
    /// </summary>
    public class RedisCacheProvider : IEnkaCache
    {
        private readonly RedisCacheOptions _options;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions? _jsonOptions;
        private long _hitCount;
        private long _missCount;
        private bool _disposed;
        private readonly bool _ownsConnection;

        /// <summary>
        /// Initializes a new instance of the RedisCacheProvider class.
        /// </summary>
        /// <param name="options">Redis cache configuration options.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        /// <exception cref="CacheException">Thrown when configuration is invalid or connection fails.</exception>
        public RedisCacheProvider(RedisCacheOptions? options = null, JsonSerializerOptions? jsonOptions = null)
        {
            _options = options ?? new RedisCacheOptions();
            _jsonOptions = jsonOptions;
            _hitCount = 0;
            _missCount = 0;
            _disposed = false;
            _ownsConnection = true;

            // Validate configuration
            _options.Validate();

            try
            {
                var configOptions = ConfigurationOptions.Parse(_options.ConnectionString);
                configOptions.ConnectRetry = _options.ConnectRetry;
                configOptions.ConnectTimeout = (int)_options.ConnectTimeout.TotalMilliseconds;
                configOptions.AbortOnConnectFail = false;

                _redis = ConnectionMultiplexer.Connect(configOptions);
                _db = _redis.GetDatabase();
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Failed to connect to Redis server at '{_options.ConnectionString}'. " +
                    "Please ensure the Redis server is running and accessible.",
                    ex);
            }
            catch (ArgumentException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Invalid Redis connection string format: {ex.Message}",
                    "ConnectionString",
                    ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the RedisCacheProvider class with an existing connection.
        /// </summary>
        /// <param name="connectionMultiplexer">An existing Redis connection multiplexer.</param>
        /// <param name="options">Redis cache configuration options.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        public RedisCacheProvider(IConnectionMultiplexer connectionMultiplexer, RedisCacheOptions? options = null, JsonSerializerOptions? jsonOptions = null)
        {
            _redis = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _options = options ?? new RedisCacheOptions();
            _jsonOptions = jsonOptions;
            _hitCount = 0;
            _missCount = 0;
            _disposed = false;
            _ownsConnection = false;

            _db = _redis.GetDatabase();
        }

        /// <summary>
        /// Gets the full Redis key with prefix.
        /// </summary>
        private string GetPrefixedKey(string key)
        {
            return string.IsNullOrEmpty(_options.KeyPrefix) ? key : $"{_options.KeyPrefix}{key}";
        }

        /// <summary>
        /// Gets the effective JSON serializer options.
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
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                Interlocked.Increment(ref _missCount);
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var prefixedKey = GetPrefixedKey(key);
                var value = await _db.StringGetAsync(prefixedKey).ConfigureAwait(false);

                if (value.IsNullOrEmpty)
                {
                    Interlocked.Increment(ref _missCount);
                    return null;
                }

                Interlocked.Increment(ref _hitCount);

                var options = GetEffectiveJsonOptions();
                try
                {
#pragma warning disable IL2026, IL3050
                    return JsonSerializer.Deserialize<T>(value.ToString(), options);
#pragma warning restore IL2026, IL3050
                }
                catch (JsonException)
                {
                    Interlocked.Decrement(ref _hitCount);
                    Interlocked.Increment(ref _missCount);
                    return null;
                }
                catch (NotSupportedException)
                {
                    try
                    {
#pragma warning disable IL2026, IL3050
                        return JsonSerializer.Deserialize<T>(value.ToString());
#pragma warning restore IL2026, IL3050
                    }
                    catch
                    {
                        Interlocked.Decrement(ref _hitCount);
                        Interlocked.Increment(ref _missCount);
                        return null;
                    }
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Failed to retrieve value from Redis for key '{key}'. Connection error.",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default) where T : class
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

            var effectiveTtl = ttl ?? _options.DefaultTtl;
            var prefixedKey = GetPrefixedKey(key);

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
#pragma warning disable IL2026, IL3050
                jsonValue = JsonSerializer.Serialize(value);
#pragma warning restore IL2026, IL3050
            }

            try
            {
                await _db.StringSetAsync(prefixedKey, jsonValue, effectiveTtl).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Failed to store value in Redis for key '{key}'. Connection error.",
                    ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var prefixedKey = GetPrefixedKey(key);
                return await _db.KeyDeleteAsync(prefixedKey).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Failed to remove value from Redis for key '{key}'. Connection error.",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var server = GetServer();
                if (server == null)
                {
                    return;
                }

                var pattern = string.IsNullOrEmpty(_options.KeyPrefix) ? "*" : $"{_options.KeyPrefix}*";

                // Use SCAN to find all keys with the prefix and delete them
                var keys = server.Keys(pattern: pattern);
                foreach (var key in keys)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _db.KeyDeleteAsync(key).ConfigureAwait(false);
                }
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    "Failed to clear Redis cache. Connection error.",
                    ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var prefixedKey = GetPrefixedKey(key);
                return await _db.KeyExistsAsync(prefixedKey).ConfigureAwait(false);
            }
            catch (RedisConnectionException ex)
            {
                throw new CacheException(
                    CacheProvider.Redis,
                    $"Failed to check existence in Redis for key '{key}'. Connection error.",
                    ex);
            }
        }

        /// <summary>
        /// Gets a Redis server instance for administrative operations.
        /// </summary>
        private IServer? GetServer()
        {
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length == 0)
            {
                return null;
            }
            return _redis.GetServer(endpoints[0]);
        }


        /// <inheritdoc/>
        public async Task<CacheStatistics> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            long entryCount = 0;
            long? sizeBytes = null;

            try
            {
                var server = GetServer();
                if (server != null)
                {
                    // Count keys with our prefix
                    var pattern = string.IsNullOrEmpty(_options.KeyPrefix) ? "*" : $"{_options.KeyPrefix}*";
                    var keys = server.Keys(pattern: pattern);
                    foreach (var _ in keys)
                    {
                        entryCount++;
                    }

                    // Get memory usage from INFO command
                    try
                    {
                        var info = await server.InfoAsync("memory").ConfigureAwait(false);
                        foreach (var group in info)
                        {
                            foreach (var pair in group)
                            {
                                if (pair.Key.Equals("used_memory", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (long.TryParse(pair.Value, out var memoryUsed))
                                    {
                                        sizeBytes = memoryUsed;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors getting memory info
                    }
                }
            }
            catch (RedisConnectionException)
            {
                // Return stats with what we have if connection fails
            }

            return new CacheStatistics
            {
                HitCount = Interlocked.Read(ref _hitCount),
                MissCount = Interlocked.Read(ref _missCount),
                EntryCount = entryCount,
                SizeBytes = sizeBytes
            };
        }


        /// <summary>
        /// Throws ObjectDisposedException if the provider has been disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RedisCacheProvider));
            }
        }

        /// <summary>
        /// Checks if the Redis connection is healthy.
        /// </summary>
        /// <returns>True if connected, false otherwise.</returns>
        public bool IsConnected => _redis?.IsConnected ?? false;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the RedisCacheProvider.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _ownsConnection)
                {
                    _redis?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
