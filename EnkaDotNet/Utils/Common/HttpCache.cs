using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace EnkaDotNet.Utils.Common
{
    public class HttpCache
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private static readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(5);
        private const int MaxCacheEntries = 1000;

        public bool TryGetValue(string cacheKey, out CacheEntry? cacheEntry)
        {
            return _cache.TryGetValue(cacheKey, out cacheEntry);
        }

        public string GenerateCacheKey(string relativeUrl)
        {
            return relativeUrl.ToLowerInvariant();
        }

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

            Console.WriteLine($"[HttpCache] Cached response for {cacheKey} until {expiration}, ETag: {etag ?? "none"}");
        }

        public void UpdateCacheEntryExpiration(string cacheKey, CacheEntry cacheEntry, HttpResponseMessage response)
        {
            cacheEntry.Expiration = CalculateExpiration(response);
            _cache[cacheKey] = cacheEntry;
        }

        public void Clear()
        {
            _cache.Clear();
            Console.WriteLine("[HttpCache] Cache cleared");
        }

        public void Remove(string cacheKey)
        {
            _cache.TryRemove(cacheKey, out _);
            Console.WriteLine($"[HttpCache] Removed from cache: {cacheKey}");
        }

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

            // Check for Expires in Content.Headers (not directly in Headers)
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

                Console.WriteLine($"[HttpCache] Pruned {oldestEntries.Count} oldest cache entries");
            }

            Console.WriteLine($"[HttpCache] Cache pruned, current count: {_cache.Count}");
        }
    }
}