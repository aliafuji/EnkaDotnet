namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Specifies the cache provider to use for storing cached data
    /// </summary>
    public enum CacheProvider
    {
        /// <summary>
        /// In-memory cache using IMemoryCache (default)
        /// </summary>
        Memory = 0,

        /// <summary>
        /// SQLite-based persistent cache for local storage
        /// </summary>
        SQLite = 1,

        /// <summary>
        /// Redis-based distributed cache for shared storage
        /// </summary>
        Redis = 2,

        /// <summary>
        /// Custom cache provider via dependency injection
        /// </summary>
        Custom = 3
    }
}
