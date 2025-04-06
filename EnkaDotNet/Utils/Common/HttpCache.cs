using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace EnkaDotNet.Utils.Common
{
    /// <summary>
    /// Provides HTTP response caching capability
    /// </summary>
    public class HttpCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly TimeSpan _defaultCacheDuration;
        private const int MaxCacheEntries = 1000;

        /// <summary>
        /// Creates a new HttpCache with the specified default cache duration
        /// </summary>
        /// <param name="defaultCacheDurationMinutes">Default cache duration in minutes</param>
        public HttpCache(int defaultCacheDurationMinutes = 5)
        {
            _defaultCacheDuration = TimeSpan.FromMinutes(defaultCacheDurationMinutes);
        }

        /// <summary>
        /// Tries to get a cached entry with the specified key
        /// </summary>
        public bool TryGetValue(string cacheKey, out CacheEntry? cacheEntry)
        {
            return _cache.TryGetValue(cacheKey, out cacheEntry);
        }

        /// <summary>
        /// Generates a cache key from a relative URL
        /// </summary>
        public string GenerateCacheKey(string relativeUrl)
        {
            return relativeUrl.ToLowerInvariant();
        }

        /// <summary>
        /// Stores a response in the cache
        /// </summary>
        public void StoreResponse(string cacheKey, string jsonResponse, HttpResponseMessage response)
        {
            if (_cache.Count > MaxCacheEntries)
            {
                PruneCache();
            }

            string? etag = response.Headers.ETag?.Tag;
            DateTimeOffset expiration = CalculateExpiration(response);

            var cacheEntry = new CacheEntry
            {
                JsonResponse = jsonResponse,
                ETag = etag,
                Expiration = expiration
            };

            _cache[cacheKey] = cacheEntry;
        }

        /// <summary>
        /// Updates the expiration of a cache entry
        /// </summary>
        public void UpdateCacheEntryExpiration(string cacheKey, CacheEntry cacheEntry, HttpResponseMessage response)
        {
            cacheEntry.Expiration = CalculateExpiration(response);
            _cache[cacheKey] = cacheEntry;
        }

        /// <summary>
        /// Clears the entire cache
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Removes a specific entry from the cache
        /// </summary>
        public void Remove(string cacheKey)
        {
            _cache.TryRemove(cacheKey, out _);
        }

        /// <summary>
        /// Gets statistics about the cache
        /// </summary>
        /// <returns>Tuple containing total count and expired count</returns>
        public (int Count, int ExpiredCount) GetStats()
        {
            int expiredCount = 0;
            foreach (var entry in _cache.Values)
            {
                if (entry.IsExpired)
                {
                    expiredCount++;
                }
            }

            return (_cache.Count, expiredCount);
        }

        private DateTimeOffset CalculateExpiration(HttpResponseMessage response)
        {
            // Check for Cache-Control header
            if (response.Headers.CacheControl?.MaxAge.HasValue == true)
            {
                return DateTimeOffset.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
            }

            // Check for Expires in Content.Headers
            if (response.Content?.Headers?.Expires != null)
            {
                return response.Content.Headers.Expires.Value;
            }

            // If no explicit cache directive is found, use default duration
            return DateTimeOffset.UtcNow.Add(_defaultCacheDuration);
        }

        private void PruneCache()
        {
            var keysToRemove = new List<string>();

            // First, remove expired entries
            foreach (var kvp in _cache)
            {
                if (kvp.Value.IsExpired)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }

            // If we still have too many items, remove oldest entries
            if (_cache.Count > MaxCacheEntries * 0.9)
            {
                var oldestEntries = _cache
                    .OrderBy(kvp => kvp.Value.Expiration)
                    .Take(_cache.Count - (int)(MaxCacheEntries * 0.8))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in oldestEntries)
                {
                    _cache.TryRemove(key, out _);
                }
            }
        }
    }
}