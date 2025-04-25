using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace EnkaDotNet.Utils.Common
{
    public class HttpCache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();
        private readonly TimeSpan _defaultCacheDuration;
        private const int MaxCacheEntries = 1000;

        public HttpCache(int defaultCacheDurationMinutes = 5)
        {
            _defaultCacheDuration = TimeSpan.FromMinutes(defaultCacheDurationMinutes);
        }

        public bool TryGetValue(string cacheKey, out CacheEntry cacheEntry)
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

            string etag = response.Headers.ETag?.Tag;
            DateTimeOffset expiration = CalculateExpiration(response);

            var cacheEntry = new CacheEntry
            {
                JsonResponse = jsonResponse,
                ETag = etag,
                Expiration = expiration
            };

            _cache[cacheKey] = cacheEntry;
        }

        public void UpdateCacheEntryExpiration(string cacheKey, CacheEntry cacheEntry, HttpResponseMessage response)
        {
            cacheEntry.Expiration = CalculateExpiration(response);
            _cache[cacheKey] = cacheEntry;
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Remove(string cacheKey)
        {
            _cache.TryRemove(cacheKey, out _);
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
            if (response.Headers.CacheControl?.MaxAge.HasValue == true)
            {
                return DateTimeOffset.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
            }

            if (response.Content?.Headers?.Expires != null)
            {
                return response.Content.Headers.Expires.Value;
            }

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
            }
        }
    }
}
