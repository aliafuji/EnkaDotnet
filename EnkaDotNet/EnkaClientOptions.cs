using EnkaDotNet.Caching;
using EnkaDotNet.Utils;
using System.Collections.Generic;
using System.Net;
using System;

namespace EnkaDotNet
{
    /// <summary>
    /// Options for configuring the EnkaDotNet client
    /// </summary>
    public class EnkaClientOptions
    {
        private int _maxRetries = 1;
        private int _retryDelayMs = 1000;
        private int _maxRetryDelayMs = 30000;
        private int _timeoutSeconds = 30;
        private int _cacheDurationMinutes = 5;

        /// <summary>
        /// Gets or sets the User-Agent string for HTTP requests
        /// </summary>
        public string UserAgent { get; set; } = Constants.DefaultUserAgent;

        /// <summary>
        /// Gets or sets the base URL for the Enka.Network API
        /// </summary>
        public string BaseUrl { get; set; } = Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL;

        /// <summary>
        /// Gets or sets the timeout for HTTP requests in seconds
        /// </summary>
        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(TimeoutSeconds), "TimeoutSeconds must be greater than 0");
                }
                _timeoutSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets the cache duration in minutes
        /// </summary>
        public int CacheDurationMinutes
        {
            get => _cacheDurationMinutes;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(CacheDurationMinutes), "CacheDurationMinutes cannot be negative");
                }
                _cacheDurationMinutes = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of retries for failed HTTP requests
        /// </summary>
        public int MaxRetries
        {
            get => _maxRetries;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetries), "MaxRetries must be at least 1");
                }
                _maxRetries = value;
            }
        }

        /// <summary>
        /// Gets or sets the delay between retries in milliseconds
        /// </summary>
        public int RetryDelayMs
        {
            get => _retryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(RetryDelayMs), "RetryDelayMs cannot be negative");
                }
                _retryDelayMs = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use exponential backoff for retries
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum retry delay in milliseconds when using exponential backoff
        /// </summary>
        public int MaxRetryDelayMs
        {
            get => _maxRetryDelayMs;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetryDelayMs), "MaxRetryDelayMs cannot be negative");
                }
                _maxRetryDelayMs = value;
            }
        }

        /// <summary>
        /// Gets or sets the list of HTTP status codes on which to retry
        /// </summary>
        public List<HttpStatusCode> RetryOnStatusCodes { get; set; } = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.BadGateway,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout,
            (HttpStatusCode)429
        };

        /// <summary>
        /// Gets or sets a value indicating whether to return raw stat values or formatted display values
        /// This affects how stats are presented in component models
        /// </summary>
        public bool Raw { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnkaClientOptions"/> class
        /// </summary>
        public EnkaClientOptions() { }

        /// <summary>
        /// Creates a shallow clone of the current options object
        /// </summary>
        public EnkaClientOptions Clone()
        {
            var clone = (EnkaClientOptions)MemberwiseClone();
            clone.RetryOnStatusCodes = new List<HttpStatusCode>(RetryOnStatusCodes);
            return clone;
        }

        /// <summary>
        /// Preloads languages for assets data
        /// </summary>
        public List<string> PreloadLanguages { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the cache provider to use for storing cached API responses.
        /// </summary>
        /// <remarks>
        /// <para>Available providers:</para>
        /// <list type="bullet">
        ///   <item><description><see cref="Caching.CacheProvider.Memory"/> - In-memory cache (default). Fast but not persistent across application restarts.</description></item>
        ///   <item><description><see cref="Caching.CacheProvider.SQLite"/> - SQLite-based persistent cache. Data survives application restarts without external dependencies.</description></item>
        ///   <item><description><see cref="Caching.CacheProvider.Redis"/> - Redis-based distributed cache. Ideal for multiple application instances sharing cache data.</description></item>
        ///   <item><description><see cref="Caching.CacheProvider.Custom"/> - Custom cache provider via dependency injection. Implement <see cref="IEnkaCache"/> interface.</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Using SQLite cache
        /// var options = new EnkaClientOptions
        /// {
        ///     CacheProvider = CacheProvider.SQLite,
        ///     SQLiteCache = new SQLiteCacheOptions
        ///     {
        ///         DatabasePath = "my_cache.db",
        ///         DefaultTtl = TimeSpan.FromMinutes(10)
        ///     }
        /// };
        /// </code>
        /// </example>
        public CacheProvider CacheProvider { get; set; } = CacheProvider.Memory;

        /// <summary>
        /// Gets or sets the SQLite cache configuration options.
        /// Only used when <see cref="CacheProvider"/> is set to <see cref="Caching.CacheProvider.SQLite"/>.
        /// </summary>
        /// <remarks>
        /// <para>Key configuration options:</para>
        /// <list type="bullet">
        ///   <item><description><see cref="SQLiteCacheOptions.DatabasePath"/> - Path to the SQLite database file (default: "enka_cache.db")</description></item>
        ///   <item><description><see cref="SQLiteCacheOptions.DefaultTtl"/> - Default time-to-live for cache entries (default: 5 minutes)</description></item>
        ///   <item><description><see cref="SQLiteCacheOptions.EnableAutoCleanup"/> - Enable automatic cleanup of expired entries (default: true)</description></item>
        ///   <item><description><see cref="SQLiteCacheOptions.CleanupInterval"/> - Interval for automatic cleanup (default: 30 minutes)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var options = new EnkaClientOptions
        /// {
        ///     CacheProvider = CacheProvider.SQLite,
        ///     SQLiteCache = new SQLiteCacheOptions
        ///     {
        ///         DatabasePath = "cache/enka_data.db",
        ///         DefaultTtl = TimeSpan.FromMinutes(15),
        ///         EnableAutoCleanup = true,
        ///         CleanupInterval = TimeSpan.FromHours(1)
        ///     }
        /// };
        /// </code>
        /// </example>
        public SQLiteCacheOptions SQLiteCache { get; set; } = new SQLiteCacheOptions();

        /// <summary>
        /// Gets or sets the Redis cache configuration options.
        /// Only used when <see cref="CacheProvider"/> is set to <see cref="Caching.CacheProvider.Redis"/>.
        /// </summary>
        /// <remarks>
        /// <para>Key configuration options:</para>
        /// <list type="bullet">
        ///   <item><description><see cref="RedisCacheOptions.ConnectionString"/> - Redis server connection string (default: "localhost:6379")</description></item>
        ///   <item><description><see cref="RedisCacheOptions.KeyPrefix"/> - Prefix for all cache keys for namespace isolation (default: "enka:")</description></item>
        ///   <item><description><see cref="RedisCacheOptions.DefaultTtl"/> - Default time-to-live for cache entries (default: 5 minutes)</description></item>
        ///   <item><description><see cref="RedisCacheOptions.ConnectRetry"/> - Number of connection retry attempts (default: 3)</description></item>
        ///   <item><description><see cref="RedisCacheOptions.ConnectTimeout"/> - Connection timeout (default: 5 seconds)</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// <code>
        /// var options = new EnkaClientOptions
        /// {
        ///     CacheProvider = CacheProvider.Redis,
        ///     RedisCache = new RedisCacheOptions
        ///     {
        ///         ConnectionString = "redis.example.com:6379,password=secret",
        ///         KeyPrefix = "myapp:enka:",
        ///         DefaultTtl = TimeSpan.FromMinutes(10),
        ///         ConnectRetry = 5
        ///     }
        /// };
        /// </code>
        /// </example>
        public RedisCacheOptions RedisCache { get; set; } = new RedisCacheOptions();
    }
}