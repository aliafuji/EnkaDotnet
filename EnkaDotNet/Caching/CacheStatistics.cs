namespace EnkaDotNet.Caching
{
    /// <summary>
    /// Represents statistics about cache performance and usage
    /// </summary>
    public class CacheStatistics
    {
        /// <summary>
        /// Gets or sets the number of cache hits (successful retrievals)
        /// </summary>
        public long HitCount { get; set; }

        /// <summary>
        /// Gets or sets the number of cache misses (failed retrievals)
        /// </summary>
        public long MissCount { get; set; }

        /// <summary>
        /// Gets or sets the current number of entries in the cache
        /// </summary>
        public long EntryCount { get; set; }

        /// <summary>
        /// Gets or sets the total size of the cache in bytes (if available)
        /// </summary>
        public long? SizeBytes { get; set; }

        /// <summary>
        /// Gets the cache hit rate as a value between 0 and 1
        /// </summary>
        public double HitRate => HitCount + MissCount > 0
            ? (double)HitCount / (HitCount + MissCount)
            : 0;
    }
}
