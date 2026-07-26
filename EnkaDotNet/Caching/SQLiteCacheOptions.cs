using System;
using System.IO;

namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Configuration options for the SQLite cache provider
    /// </summary>
    public class SQLiteCacheOptions
    {
        /// <summary>
        /// Gets or sets the path to the SQLite database file
        /// </summary>
        public string DatabasePath { get; set; } = "enka_cache.db";

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
        /// Gets or sets the interval for automatic cleanup of expired entries
        /// </summary>
        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets whether automatic cleanup of expired entries is enabled
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// Validates the configuration options and throws an exception if invalid.
        /// </summary>
        /// <exception cref="Exceptions.CacheException">Thrown when configuration is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DatabasePath))
            {
                throw new Exceptions.CacheException(
                    CacheProvider.SQLite,
                    "SQLite database path cannot be null or empty.",
                    "DatabasePath");
            }

            // Check for invalid path characters
            var invalidChars = Path.GetInvalidPathChars();
            foreach (var c in invalidChars)
            {
                if (DatabasePath.IndexOf(c) >= 0)
                {
                    throw new Exceptions.CacheException(
                        CacheProvider.SQLite,
                        $"SQLite database path contains invalid character: '{c}'.",
                        "DatabasePath");
                }
            }

            // Check if the path has a valid file name
            try
            {
                var fileName = Path.GetFileName(DatabasePath);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new Exceptions.CacheException(
                        CacheProvider.SQLite,
                        "SQLite database path must include a file name.",
                        "DatabasePath");
                }

                var invalidFileNameChars = Path.GetInvalidFileNameChars();
                foreach (var c in invalidFileNameChars)
                {
                    if (fileName.IndexOf(c) >= 0)
                    {
                        throw new Exceptions.CacheException(
                            CacheProvider.SQLite,
                            $"SQLite database file name contains invalid character: '{c}'.",
                            "DatabasePath");
                    }
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.SQLite,
                    $"SQLite database path is invalid: {ex.Message}",
                    "DatabasePath",
                    ex);
            }

            // Relative traversal is rejected outright: the resolved location is nowhere near
            // obvious from the configured string, and the provider creates directories for it.
            var segments = DatabasePath.Split(new[] { '/', '\\' }, StringSplitOptions.None);
            if (System.Linq.Enumerable.Any(segments, segment => segment == ".."))
            {
                throw new Exceptions.CacheException(
                    CacheProvider.SQLite,
                    "SQLite database path cannot contain '..' segments. Use an absolute path or a " +
                    "path relative to the working directory without traversal.",
                    "DatabasePath");
            }

            if (DefaultTtl <= TimeSpan.Zero)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.SQLite,
                    "Default TTL must be greater than zero.",
                    "DefaultTtl");
            }

            if (EnableAutoCleanup && CleanupInterval <= TimeSpan.Zero)
            {
                throw new Exceptions.CacheException(
                    CacheProvider.SQLite,
                    "Cleanup interval must be greater than zero when auto cleanup is enabled.",
                    "CleanupInterval");
            }
        }
    }
}
