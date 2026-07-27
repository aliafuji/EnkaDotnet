using System;

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Configuration options for the Redis cache provider
    /// </summary>
    public class RedisCacheOptions
    {
        private static readonly char[] _globMetacharacters = { '*', '?', '[', ']' };

        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        /// <remarks>
        /// The default connects to a local server over plaintext. For any server reached across a
        /// network, add <c>ssl=true</c> (and a password) to the connection string, for example
        /// <c>redis.example.com:6380,ssl=true,password=...</c>. The provider passes the string to
        /// <c>ConfigurationOptions.Parse</c> unchanged and does not enable TLS on your behalf.
        /// <para>
        /// <c>AbortOnConnectFail</c> is forced to <c>false</c> so a temporarily unreachable server
        /// does not fail application startup; operations will surface the error instead.
        /// </para>
        /// </remarks>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// Gets or sets the key prefix for namespace isolation. Must be non empty: the prefix
        /// scopes <c>ClearAsync</c> and statistics, so an empty prefix would target every key on
        /// the server, including keys owned by other applications.
        /// </summary>
        public string KeyPrefix { get; set; } = "enka:";

        private TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the default time-to-live for cache entries
        /// </summary>
        public TimeSpan DefaultTtl
        {
            get => _defaultTtl;
            set
            {
                _defaultTtl = value;
                ExplicitDefaultTtl = value;
            }
        }

        /// <summary>
        /// The value assigned to <see cref="DefaultTtl"/>, or <c>null</c> when it was never set.
        /// Lets <see cref="CacheFactory"/> fall back to
        /// <see cref="EnkaClientOptions.CacheDurationMinutes"/> without having to guess whether a
        /// five minute TTL was deliberate.
        /// </summary>
        internal TimeSpan? ExplicitDefaultTtl { get; private set; }

        /// <summary>
        /// Gets or sets the number of connection retry attempts
        /// </summary>
        public int ConnectRetry { get; set; } = 3;

        /// <summary>
        /// Gets or sets the connection timeout
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Validates the configuration options and throws an exception if invalid.
        /// </summary>
        /// <exception cref="Exceptions.CacheException">Thrown when configuration is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Redis connection string cannot be null or empty.",
                    "ConnectionString");
            }

            // Basic connection string format validation
            // Redis connection strings typically contain host:port or host,host:port format
            // They can also contain options like password=xxx,ssl=true
            if (!IsValidConnectionStringFormat(ConnectionString))
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Redis connection string format is invalid. Expected format: 'host:port' or 'host:port,option=value'.",
                    "ConnectionString");
            }

            if (string.IsNullOrEmpty(KeyPrefix))
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Redis key prefix cannot be null or empty, because it scopes ClearAsync and " +
                    "statistics to this application's keys.",
                    "KeyPrefix");
            }

            if (KeyPrefix.IndexOfAny(_globMetacharacters) >= 0)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Redis key prefix cannot contain the glob metacharacters '*', '?', '[' or ']'.",
                    "KeyPrefix");
            }

            if (DefaultTtl <= TimeSpan.Zero)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Default TTL must be greater than zero.",
                    "DefaultTtl");
            }

            if (ConnectRetry < 0)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Connect retry count cannot be negative.",
                    "ConnectRetry");
            }

            if (ConnectTimeout <= TimeSpan.Zero)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.Redis,
                    "Connect timeout must be greater than zero.",
                    "ConnectTimeout");
            }
        }

        /// <summary>
        /// Validates the basic format of a Redis connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to validate.</param>
        /// <returns>True if the format appears valid, false otherwise.</returns>
        private static bool IsValidConnectionStringFormat(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            // Split by comma to get individual parts (host:port pairs and options)
            var parts = connectionString.Split(',');
            if (parts.Length == 0)
            {
                return false;
            }

            // The first part should be a host:port or just host
            var firstPart = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(firstPart))
            {
                return false;
            }

            // Check if it's an option (contains =) - if so, it's invalid as first part
            if (firstPart.IndexOf('=') >= 0 && firstPart.IndexOf(':') < 0)
            {
                // Could be a named endpoint like "endpoint=xxx" which is valid
                // But a simple "option=value" without host is not valid
                if (!firstPart.StartsWith("endpoint", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // If it contains a colon, validate port number (if it's host:port format)
            if (firstPart.IndexOf(':') >= 0 && firstPart.IndexOf('=') < 0)
            {
                var hostPort = firstPart.Split(':');
                if (hostPort.Length == 2)
                {
                    // Validate port is a number
                    if (!int.TryParse(hostPort[1], out var port) || port < 0 || port > 65535)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
