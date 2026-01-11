using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Caching;
using EnkaDotNet.Caching.Providers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Polly;
using Polly.Retry;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

#nullable enable

namespace EnkaDotNet.Utils.Common
{
    public class HttpHelper : IHttpHelper
    {
        private readonly HttpClient _httpClient;
        private readonly IEnkaCache _cache;
        private readonly IMemoryCache? _legacyMemoryCache;
        private readonly EnkaClientOptions _options;
        private readonly ILogger<HttpHelper> _logger;
        private bool _disposed = false;
        private readonly ConcurrentDictionary<string, bool> _trackedCacheKeys;
        private ResiliencePipeline<HttpResponseMessage> _resiliencePipeline;
        private static readonly Random _jitterer = new Random();
        private static readonly ResiliencePropertyKey<string> RelativeUrlKey = new ResiliencePropertyKey<string>("relativeUrl");

        /// <summary>
        /// Initializes a new instance of the HttpHelper class with IEnkaCache support.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making requests.</param>
        /// <param name="cache">The cache provider to use.</param>
        /// <param name="options">The client options.</param>
        /// <param name="logger">The logger instance.</param>
        public HttpHelper(
            HttpClient httpClient,
            IEnkaCache cache,
            IOptions<EnkaClientOptions> options,
            ILogger<HttpHelper> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _legacyMemoryCache = null;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<HttpHelper>.Instance;
            _trackedCacheKeys = new ConcurrentDictionary<string, bool>();
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            InitializeResiliencePipeline();
        }

        /// <summary>
        /// Initializes a new instance of the HttpHelper class with IMemoryCache for backward compatibility.
        /// </summary>
        /// <param name="httpClient">The HTTP client for making requests.</param>
        /// <param name="memoryCache">The memory cache instance (legacy support).</param>
        /// <param name="options">The client options.</param>
        /// <param name="logger">The logger instance.</param>
        public HttpHelper(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IOptions<EnkaClientOptions> options,
            ILogger<HttpHelper> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _legacyMemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<HttpHelper>.Instance;
            _trackedCacheKeys = new ConcurrentDictionary<string, bool>();
            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

            // Wrap the IMemoryCache in a MemoryCacheAdapter for unified interface
            var defaultTtl = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
            _cache = new MemoryCacheAdapter(memoryCache, defaultTtl);

            InitializeResiliencePipeline();
        }

        /// <summary>
        /// Initializes the resilience pipeline for HTTP requests.
        /// </summary>
        private void InitializeResiliencePipeline()

        {
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r =>
                    {
                        foreach (var code in _options.RetryOnStatusCodes)
                        {
                            if (r.StatusCode == code && r.StatusCode != (HttpStatusCode)429)
                            {
                                return true;
                            }
                        }
                        return false;
                    }),
                MaxRetryAttempts = _options.MaxRetries,
                DelayGenerator = args =>
                {
                    TimeSpan delay = TimeSpan.FromMilliseconds(_options.RetryDelayMs);
                    if (_options.UseExponentialBackoff)
                    {
                        delay = TimeSpan.FromMilliseconds(Math.Min(Math.Pow(2, args.AttemptNumber) * _options.RetryDelayMs, _options.MaxRetryDelayMs));
                    }
                    delay += TimeSpan.FromMilliseconds(_jitterer.Next(0, (int)(delay.TotalMilliseconds * 0.2)));
                    return new ValueTask<TimeSpan?>(delay);
                },
                OnRetry = args =>
                {
                    string urlForLog = "Unknown URL";
                    if (args.Context.Properties.TryGetValue(RelativeUrlKey, out var url))
                    {
                        urlForLog = url;
                    }
                    _logger.LogWarning(args.Outcome.Exception,
                        "Request to {Url} failed. Delaying for {Delay}, then making retry {AttemptNumber}/{MaxRetries}. Status: {StatusCode}",
                        urlForLog, args.RetryDelay, args.AttemptNumber + 1, _options.MaxRetries, args.Outcome.Result?.StatusCode);
                    return new ValueTask();
                }
            };

            _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .Build();
        }

        public async Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpHelper));

            string cacheKey = relativeUrl.ToLowerInvariant();

            if (_options.EnableCaching && !bypassCache)
            {
                var cachedEntry = await _cache.GetAsync<CacheEntry>(cacheKey, cancellationToken).ConfigureAwait(false);
                if (cachedEntry != null && !cachedEntry.IsExpired)
                {
                    _logger.Log(LogLevel.Trace, "Cache hit for {Url}", relativeUrl);
                    _trackedCacheKeys.TryAdd(cacheKey, true);
#if NET8_0_OR_GREATER
                    return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse, EnkaJsonContext.Default.Options) ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} (result was null)");
#else
#pragma warning disable IL2026, IL3050
                    return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse) ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} (result was null)");
#pragma warning restore IL2026, IL3050
#endif
                }
            }

            HttpResponseMessage response = null;
            string jsonString = null;
            ResilienceContext resilienceContext = null;

            try
            {
                resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);
                resilienceContext.Properties.Set(RelativeUrlKey, relativeUrl);

                response = await _resiliencePipeline.ExecuteAsync(
                    async (ctx, ct) =>
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get, ctx.Properties.GetValue(RelativeUrlKey, "ERROR_NO_URL_IN_CONTEXT")))
                        {
                            _logger.Log(LogLevel.Trace, "Sending request via Polly to {BaseAddress}{Url}.", _httpClient.BaseAddress, ctx.Properties.GetValue(RelativeUrlKey, ""));
                            HttpResponseMessage httpResponse = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

                            if (httpResponse.StatusCode == (HttpStatusCode)429)
                            {
                                TimeSpan retryAfterDelay = GetRetryAfterDelay(httpResponse.Headers.RetryAfter, TimeSpan.FromSeconds(_options.RetryDelayMs));
                                _logger.LogWarning("API returned 429 Too Many Requests for URL: {Url}. Throwing RateLimitException. Retry-After: {Delay}.", ctx.Properties.GetValue(RelativeUrlKey, ""), retryAfterDelay);
                                throw new RateLimitException($"API rate limit exceeded for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}. Retry-After: {httpResponse.Headers.RetryAfter}", httpResponse.Headers.RetryAfter);
                            }

                            if (httpResponse.StatusCode == (HttpStatusCode)424)
                            {
                                throw new GameMaintenanceException($"API returned 424 Failed Dependency for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}. Game may be under maintenance or API access restricted.");
                            }

                            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                            {
                                int uid = ExtractUidFromUrl(ctx.Properties.GetValue(RelativeUrlKey, ""));
                                throw new PlayerNotFoundException(uid, $"API returned 404 Not Found for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}");
                            }
                            if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                            {
                                int uid = ExtractUidFromUrl(ctx.Properties.GetValue(RelativeUrlKey, ""));
                                throw new ProfilePrivateException(uid, $"API returned 403 Forbidden for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}. Profile may be private or API access restricted.");
                            }
                            return httpResponse;
                        }
                    },
                    resilienceContext
                ).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                response.EnsureSuccessStatusCode();

                jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.Log(LogLevel.Trace, "Received {ByteCount} bytes from {Url}", jsonString.Length, relativeUrl);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    _logger.LogWarning("Received empty or whitespace JSON response from {Url}", relativeUrl);
                }

                cancellationToken.ThrowIfCancellationRequested();
#if NET8_0_OR_GREATER
                T deserializedObject = JsonSerializer.Deserialize<T>(jsonString, EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                T deserializedObject = JsonSerializer.Deserialize<T>(jsonString);
#pragma warning restore IL2026, IL3050
#endif
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
                if (resilienceContext != null)
                {
                    ResilienceContextPool.Shared.Return(resilienceContext);
                }
            }
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

        private async void CacheSuccessfulResponse(string cacheKey, string jsonString, HttpResponseMessage response, string relativeUrl)
        {
            DateTimeOffset expiration = CalculateExpiration(response, _options.CacheDurationMinutes);
            var newCacheEntry = new CacheEntry
            {
                JsonResponse = jsonString,
                Expiration = expiration
            };
            
            var ttl = expiration - DateTimeOffset.UtcNow;
            if (ttl > TimeSpan.Zero)
            {
                try
                {
                    await _cache.SetAsync(cacheKey, newCacheEntry, ttl).ConfigureAwait(false);
                    _trackedCacheKeys.TryAdd(cacheKey, true);
                    _logger.Log(LogLevel.Trace, "Stored response in cache for {Url} with expiration: {Expiration}", relativeUrl, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache response for {Url}", relativeUrl);
                }
            }
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
            ClearCacheAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Clears all entries from the cache asynchronously.
        /// </summary>
        public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.ClearAsync(cancellationToken).ConfigureAwait(false);
                _trackedCacheKeys.Clear();
                _logger.LogInformation("Cleared all entries from cache.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear cache, falling back to tracked keys removal.");
                
                // Fallback: remove tracked keys individually
                foreach (var key in _trackedCacheKeys.Keys)
                {
                    try
                    {
                        await _cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore individual removal errors
                    }
                }
                _trackedCacheKeys.Clear();
            }
        }

        public void RemoveFromCache(string relativeUrl)
        {
            RemoveFromCacheAsync(relativeUrl).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Removes a specific entry from the cache asynchronously.
        /// </summary>
        public async Task RemoveFromCacheAsync(string relativeUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            string cacheKey = relativeUrl.ToLowerInvariant();
            
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                _trackedCacheKeys.TryRemove(cacheKey, out _);
                _logger.Log(LogLevel.Trace, "Removed {CacheKey} from cache.", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove {CacheKey} from cache.", cacheKey);
            }
        }

        public (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats()
        {
            return GetCacheStatsAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets cache statistics asynchronously.
        /// </summary>
        public async Task<(long CurrentEntryCount, int ExpiredCountNotAvailable)> GetCacheStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _cache.GetStatsAsync(cancellationToken).ConfigureAwait(false);
                _logger.Log(LogLevel.Trace, "Cache stats - Entries: {Count}, Hits: {Hits}, Misses: {Misses}, HitRate: {HitRate:P2}", 
                    stats.EntryCount, stats.HitCount, stats.MissCount, stats.HitRate);
                return (stats.EntryCount, 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache stats, returning tracked keys count.");
                return (_trackedCacheKeys.Count, 0);
            }
        }

        /// <summary>
        /// Gets detailed cache statistics.
        /// </summary>
        public async Task<CacheStatistics> GetDetailedCacheStatsAsync(CancellationToken cancellationToken = default)
        {
            return await _cache.GetStatsAsync(cancellationToken).ConfigureAwait(false);
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
                    // Dispose the cache if we own it (not the legacy memory cache)
                    if (_legacyMemoryCache == null)
                    {
                        _cache?.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}
