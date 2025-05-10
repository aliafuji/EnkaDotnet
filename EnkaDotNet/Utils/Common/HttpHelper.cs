using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Exceptions;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Polly;
using Polly.Retry;

namespace EnkaDotNet.Utils.Common
{
    public class HttpHelper : IHttpHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly EnkaClientOptions _options;
        private readonly ILogger<HttpHelper> _logger;
        private bool _disposed = false;
        private readonly ConcurrentDictionary<string, bool> _trackedCacheKeys;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private static readonly Random _jitterer = new Random();

        public HttpHelper(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IOptions<EnkaClientOptions> options,
            ILogger<HttpHelper> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<HttpHelper>.Instance;
            _trackedCacheKeys = new ConcurrentDictionary<string, bool>();

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => _options.RetryOnStatusCodes.Contains(r.StatusCode) && r.StatusCode != (HttpStatusCode)429)
                .WaitAndRetryAsync(
                    _options.MaxRetries,
                    retryAttempt =>
                    {
                        TimeSpan delay = TimeSpan.FromMilliseconds(_options.RetryDelayMs);
                        if (_options.UseExponentialBackoff)
                        {
                            delay = TimeSpan.FromMilliseconds(Math.Min(Math.Pow(2, retryAttempt) * _options.RetryDelayMs, _options.MaxRetryDelayMs));
                        }
                        delay += TimeSpan.FromMilliseconds(_jitterer.Next(0, (int)(delay.TotalMilliseconds * 0.2)));
                        return delay;
                    },
                    (outcome, timespan, retryAttempt, context) =>
                    {
                        string urlForLog = context.TryGetValue("relativeUrl", out var urlObj) && urlObj is string url ? url : "Unknown URL";
                        _logger.LogWarning(outcome.Exception,
                            "Request to {Url} failed. Delaying for {Timespan}, then making retry {RetryAttempt}/{MaxRetries}. Status: {StatusCode}",
                            urlForLog, timespan, retryAttempt, _options.MaxRetries, outcome.Result?.StatusCode);
                    }
                );
        }

        public async Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpHelper));

            string cacheKey = relativeUrl.ToLowerInvariant();
            CacheEntry cachedEntry = null;

            if (_options.EnableCaching && !bypassCache)
            {
                if (_memoryCache.TryGetValue(cacheKey, out cachedEntry) && cachedEntry != null)
                {
                    _logger.Log(LogLevel.Trace, "Cache hit for {Url}", relativeUrl);
                    _trackedCacheKeys.TryAdd(cacheKey, true);
                }
                else
                {
                    cachedEntry = null;
                }
            }

            HttpResponseMessage response = null;
            string jsonString = null;

            try
            {
                response = await SendRequestWithPolicyHandlingAsync(relativeUrl, cachedEntry, bypassCache, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotModified && cachedEntry != null && _options.EnableCaching && !bypassCache)
                {
                    _logger.Log(LogLevel.Trace, "304 Not Modified for {Url}, using cached data and updating expiration.", relativeUrl);
                    UpdateCacheEntryExpiration(cacheKey, cachedEntry, response);
                    return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse) ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} after 304 (result was null)");
                }

                response.EnsureSuccessStatusCode();

                jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.Log(LogLevel.Trace, "Received {ByteCount} bytes from {Url}", jsonString.Length, relativeUrl);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    _logger.LogWarning("Received empty or whitespace JSON response from {Url}", relativeUrl);
                }

                T deserializedObject = JsonSerializer.Deserialize<T>(jsonString);
                if (deserializedObject == null && !string.IsNullOrWhiteSpace(jsonString))
                {
                    throw new EnkaNetworkException($"Failed to deserialize JSON response from {relativeUrl}, but content was not empty. Result was null.");
                }

                if (response.IsSuccessStatusCode && _options.EnableCaching)
                {
                    CacheSuccessfulResponse(cacheKey, jsonString, response, relativeUrl);
                }
                return deserializedObject;
            }
            catch (JsonException ex)
            {
                string snippet = jsonString?.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString ?? "Response content was null or unreadable.";
                _logger.LogError(ex, "Failed to parse JSON response from {Url}. Snippet: {Snippet}", relativeUrl, snippet);
                throw new EnkaNetworkException($"Failed to parse JSON response from {relativeUrl}. Snippet: {snippet}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        private async Task<HttpResponseMessage> SendRequestWithPolicyHandlingAsync(string relativeUrl, CacheEntry cachedEntry, bool bypassCache, CancellationToken cancellationToken)
        {
            Context pollyContext = new Context();
            pollyContext.Add("relativeUrl", relativeUrl);

            return await _retryPolicy.ExecuteAsync(async (ctx, ct) =>
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl))
                {
                    if (cachedEntry != null && !string.IsNullOrEmpty(cachedEntry.ETag) && _options.EnableCaching && !bypassCache)
                    {
                        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cachedEntry.ETag));
                    }

                    _logger.Log(LogLevel.Trace, "Sending request via Polly to {BaseAddress}{Url}.", _httpClient.BaseAddress, relativeUrl);
                    HttpResponseMessage response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        TimeSpan retryAfterDelay = GetRetryAfterDelay(response.Headers.RetryAfter, TimeSpan.FromSeconds(_options.RetryDelayMs));
                        _logger.LogWarning("API returned 429 Too Many Requests for URL: {Url}. Will not retry via Polly, throwing RateLimitException. Consider handling Retry-After: {Delay}.", relativeUrl, retryAfterDelay);
                        throw new RateLimitException($"API rate limit exceeded for URL: {relativeUrl}. Retry-After: {response.Headers.RetryAfter}", response.Headers.RetryAfter);
                    }

                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        int uid = ExtractUidFromUrl(relativeUrl);
                        throw new PlayerNotFoundException(uid, $"API returned 404 Not Found for URL: {relativeUrl}");
                    }
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        int uid = ExtractUidFromUrl(relativeUrl);
                        throw new ProfilePrivateException(uid, $"API returned 403 Forbidden for URL: {relativeUrl}. Profile may be private or API access restricted.");
                    }
                    return response;
                }
            }, pollyContext, cancellationToken).ConfigureAwait(false);
        }

        private TimeSpan GetRetryAfterDelay(RetryConditionHeaderValue retryAfterHeader, TimeSpan defaultDelay)
        {
            if (retryAfterHeader != null)
            {
                if (retryAfterHeader.Delta.HasValue)
                {
                    return retryAfterHeader.Delta.Value > TimeSpan.Zero ? retryAfterHeader.Delta.Value : defaultDelay;
                }
                if (retryAfterHeader.Date.HasValue)
                {
                    var delay = retryAfterHeader.Date.Value - DateTimeOffset.UtcNow;
                    return delay > TimeSpan.Zero ? delay : defaultDelay;
                }
            }
            return defaultDelay;
        }

        private void UpdateCacheEntryExpiration(string cacheKey, CacheEntry cachedEntry, HttpResponseMessage response)
        {
            var newExpiration = CalculateExpiration(response, _options.CacheDurationMinutes);
            cachedEntry.Expiration = newExpiration.UtcDateTime;
            var updatedEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(newExpiration);
            _memoryCache.Set(cacheKey, cachedEntry, updatedEntryOptions);
            _trackedCacheKeys.TryAdd(cacheKey, true);
        }

        private void CacheSuccessfulResponse(string cacheKey, string jsonString, HttpResponseMessage response, string relativeUrl)
        {
            string etag = response.Headers.ETag?.Tag;
            DateTimeOffset expiration = CalculateExpiration(response, _options.CacheDurationMinutes);
            var newCacheEntry = new CacheEntry
            {
                JsonResponse = jsonString,
                ETag = etag,
                Expiration = expiration.UtcDateTime
            };
            var entryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration);
            _memoryCache.Set(cacheKey, newCacheEntry, entryOptions);
            _trackedCacheKeys.TryAdd(cacheKey, true);
            _logger.Log(LogLevel.Trace, "Stored response in cache for {Url} with ETag: {ETag} and expiration: {Expiration}", relativeUrl, etag, expiration);
        }

        private static DateTimeOffset CalculateExpiration(HttpResponseMessage response, int defaultCacheDurationMinutes)
        {
            if (response.Headers.CacheControl?.MaxAge.HasValue == true)
            {
                return DateTimeOffset.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
            }
            if (response.Content?.Headers?.Expires != null)
            {
                var expiresHeader = response.Content.Headers.Expires.Value;
                if (expiresHeader > DateTimeOffset.UtcNow) return expiresHeader;
            }
            return DateTimeOffset.UtcNow.AddMinutes(defaultCacheDurationMinutes);
        }

        public void ClearCache()
        {
            var keysToRemove = _trackedCacheKeys.Keys.ToList();
            int clearedCount = 0;
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _trackedCacheKeys.TryRemove(key, out _);
                clearedCount++;
            }
            _logger.LogInformation("Cleared {ClearedCount} tracked entries from MemoryCache.", clearedCount);

            if (_memoryCache is MemoryCache concreteCache)
            {
                concreteCache.Compact(1.0);
                _logger.LogInformation("Additionally attempted to clear MemoryCache by compacting 100% of entries.");
            }
            else
            {
                _logger.LogWarning("The IMemoryCache instance is not a concrete MemoryCache. Full compaction not performed, only tracked keys removed.");
            }
        }

        public void RemoveFromCache(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            string cacheKey = relativeUrl.ToLowerInvariant();
            _memoryCache.Remove(cacheKey);
            _trackedCacheKeys.TryRemove(cacheKey, out _);
            _logger.Log(LogLevel.Trace, "Removed {CacheKey} from MemoryCache and tracking.", cacheKey);
        }

        public (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats()
        {
            long count = 0;
            if (_memoryCache is MemoryCache concreteCache)
            {
                count = concreteCache.Count;
            }
            _logger.Log(LogLevel.Trace, "MemoryCache current entry count (approximate): {Count}. Tracked keys by HttpHelper: {TrackedKeyCount}. Detailed expired count not available.", count, _trackedCacheKeys.Count);
            return (count, 0);
        }

        private int ExtractUidFromUrl(string url)
        {
            var parts = url?.Split('/');
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    bool isUidSegment = parts[i].Equals("uid", StringComparison.OrdinalIgnoreCase);
                    if (isUidSegment && i + 1 < parts.Length && int.TryParse(parts[i + 1], out int uidAfterSegment)) return uidAfterSegment;
                    if (i == parts.Length - 1 && int.TryParse(parts[i], out int uidLastSegment)) return uidLastSegment;
                    if (i == 0 && int.TryParse(parts[i], out int uidFirstSegment)) return uidFirstSegment;
                }
            }
            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }
    }
}
