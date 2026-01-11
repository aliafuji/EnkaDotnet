using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using EnkaDotNet.Exceptions;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

#nullable enable

namespace EnkaDotNet.Caching.Providers
{
    /// <summary>
    /// SQLite-based cache provider for persistent caching.
    /// Stores cache entries in a local SQLite database file.
    /// </summary>
    public class SQLiteCacheProvider : IEnkaCache
    {
        private readonly SQLiteCacheOptions _options;
        private readonly string _connectionString;
        private readonly JsonSerializerOptions? _jsonOptions;
        private readonly Timer? _cleanupTimer;
        private long _hitCount;
        private long _missCount;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the SQLiteCacheProvider class.
        /// </summary>
        /// <param name="options">SQLite cache configuration options.</param>
        /// <param name="jsonOptions">Optional JSON serializer options.</param>
        /// <exception cref="CacheException">Thrown when configuration is invalid.</exception>
        public SQLiteCacheProvider(SQLiteCacheOptions? options = null, JsonSerializerOptions? jsonOptions = null)
        {
            _options = options ?? new SQLiteCacheOptions();
            _jsonOptions = jsonOptions;
            _hitCount = 0;
            _missCount = 0;
            _disposed = false;

            // Validate configuration
            _options.Validate();

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_options.DatabasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    throw new CacheException(
                        CacheProvider.SQLite,
                        $"Failed to create directory for database: {ex.Message}",
                        "DatabasePath",
                        ex);
                }
            }

            _connectionString = $"Data Source={_options.DatabasePath}";

            try
            {
                InitializeDatabase();
            }
            catch (SqliteException ex)
            {
                throw new CacheException(
                    CacheProvider.SQLite,
                    $"Failed to initialize SQLite database: {ex.Message}",
                    ex);
            }

            if (_options.EnableAutoCleanup)
            {
                _cleanupTimer = new Timer(
                    CleanupExpiredEntries,
                    null,
                    _options.CleanupInterval,
                    _options.CleanupInterval);
            }
        }

        /// <summary>
        /// Initializes the SQLite database and creates the cache table if it doesn't exist.
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS cache_entries (
                    key TEXT PRIMARY KEY,
                    value TEXT NOT NULL,
                    expiration INTEGER NOT NULL,
                    created_at INTEGER NOT NULL
                );
                CREATE INDEX IF NOT EXISTS idx_expiration ON cache_entries(expiration);
            ";
            command.ExecuteNonQuery();
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

            string? jsonValue = null;
            bool isExpired = false;

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT value, expiration FROM cache_entries WHERE key = @key
                ";
                command.Parameters.AddWithValue("@key", key);

                using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var expirationTicks = reader.GetInt64(1);
                    var expiration = new DateTime(expirationTicks, DateTimeKind.Utc);

                    if (DateTime.UtcNow > expiration)
                    {
                        isExpired = true;
                    }
                    else
                    {
                        jsonValue = reader.GetString(0);
                    }
                }
            }

            if (isExpired)
            {
                Interlocked.Increment(ref _missCount);
                await RemoveExpiredEntryAsync(key, cancellationToken).ConfigureAwait(false);
                return null;
            }

            if (jsonValue == null)
            {
                Interlocked.Increment(ref _missCount);
                return null;
            }

            Interlocked.Increment(ref _hitCount);

            try
            {
                var options = GetEffectiveJsonOptions();
#pragma warning disable IL2026, IL3050
                return JsonSerializer.Deserialize<T>(jsonValue, options);
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
                    return JsonSerializer.Deserialize<T>(jsonValue);
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
            var expiration = DateTime.UtcNow.Add(effectiveTtl);
            var createdAt = DateTime.UtcNow;

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

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO cache_entries (key, value, expiration, created_at)
                VALUES (@key, @value, @expiration, @created_at)
            ";
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", jsonValue);
            command.Parameters.AddWithValue("@expiration", expiration.Ticks);
            command.Parameters.AddWithValue("@created_at", createdAt.Ticks);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes an expired entry from the database.
        /// </summary>
        private async Task RemoveExpiredEntryAsync(string key, CancellationToken cancellationToken)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM cache_entries WHERE key = @key";
            command.Parameters.AddWithValue("@key", key);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM cache_entries WHERE key = @key";
            command.Parameters.AddWithValue("@key", key);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return rowsAffected > 0;
        }

        /// <inheritdoc/>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM cache_entries";
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 1 FROM cache_entries WHERE key = @key AND expiration > @now LIMIT 1
            ";
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);

            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result != null;
        }

        /// <summary>
        /// Timer callback to cleanup expired entries.
        /// </summary>
        private void CleanupExpiredEntries(object? state)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM cache_entries WHERE expiration < @now";
                command.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);
                command.ExecuteNonQuery();
            }
            catch
            {
                // Silently ignore cleanup errors to avoid crashing the timer
            }
        }

        /// <inheritdoc/>
        public async Task<CacheStatistics> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            long entryCount = 0;
            long? sizeBytes = null;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            // Get entry count (only non-expired entries)
            using (var countCommand = connection.CreateCommand())
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM cache_entries WHERE expiration > @now";
                countCommand.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);
                var result = await countCommand.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                entryCount = Convert.ToInt64(result);
            }

            // Get database file size
            try
            {
                if (File.Exists(_options.DatabasePath))
                {
                    var fileInfo = new FileInfo(_options.DatabasePath);
                    sizeBytes = fileInfo.Length;
                }
            }
            catch
            {
                // Ignore file access errors
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
                throw new ObjectDisposedException(nameof(SQLiteCacheProvider));
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SQLiteCacheProvider.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cleanupTimer?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
