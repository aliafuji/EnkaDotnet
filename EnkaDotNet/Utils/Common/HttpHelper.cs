using System;
using System.Diagnostics;
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
using Polly.CircuitBreaker;
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
        private ResiliencePipeline<HttpResponseMessage> _resiliencePipeline = null!;

        private static readonly ResiliencePropertyKey<string> RelativeUrlKey = new ResiliencePropertyKey<string>("relativeUrl");

        /// <summary>
        /// Initializes a new instance of the HttpHelper class.
        /// </summary>
        public HttpHelper(
            HttpClient httpClient,
            IOptions<EnkaClientOptions> options,
            ILogger<HttpHelper> logger,
            IEnkaCache? cache = null,
            IMemoryCache? memoryCache = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? NullLogger<HttpHelper>.Instance;
            _trackedCacheKeys = new ConcurrentDictionary<string, bool>();

            if (cache != null)
            {
                _cache = cache;
                _legacyMemoryCache = null;
            }
            else if (memoryCache != null)
            {
                _legacyMemoryCache = memoryCache;
                var defaultTtl = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
                _cache = new MemoryCacheAdapter(memoryCache, defaultTtl);
            }
            else
            {
                _legacyMemoryCache = null;
                _cache = new MemoryCacheAdapter(new MemoryCache(new MemoryCacheOptions()), TimeSpan.FromMinutes(_options.CacheDurationMinutes));
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
            InitializeResiliencePipeline();
        }


        private void InitializeResiliencePipeline()
        {
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(IsRetryableStatusCode),
                MaxRetryAttempts = _options.MaxRetries,
                DelayGenerator = ComputeRetryDelay,
                OnRetry = OnRetryAttempt
            };

            var circuitBreakerOptions = new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(IsCircuitBreakerStatusCode),
                FailureRatio = 1.0,
                MinimumThroughput = _options.CircuitBreakerFailureThreshold,
                SamplingDuration = TimeSpan.FromSeconds(60),
                BreakDuration = TimeSpan.FromSeconds(_options.CircuitBreakerBreakDurationSeconds),
                OnOpened = args =>
                {
                    _logger.LogWarning(EnkaEventIds.CircuitOpen,
                        "Circuit breaker opened. Fast-failing for {BreakDuration}s.",
                        args.BreakDuration.TotalSeconds);
                    return new ValueTask();
                },
                OnClosed = args =>
                {
                    _logger.LogInformation(EnkaEventIds.CircuitClosed, "Circuit breaker closed.");
                    return new ValueTask();
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation(EnkaEventIds.CircuitHalfOpen, "Circuit breaker half-open, probing.");
                    return new ValueTask();
                }
            };

            _resiliencePipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .AddCircuitBreaker(circuitBreakerOptions)
                .Build();
        }

        private bool IsRetryableStatusCode(HttpResponseMessage response)
        {
            foreach (var code in _options.RetryOnStatusCodes)
            {
                if (response.StatusCode == code) return true;
            }
            return false;
        }

        private bool IsCircuitBreakerStatusCode(HttpResponseMessage response)
        {
            foreach (var code in _options.RetryOnStatusCodes)
            {
                if (response.StatusCode == code && response.StatusCode != (HttpStatusCode)429) return true;
            }
            return false;
        }

        private ValueTask<TimeSpan?> ComputeRetryDelay(RetryDelayGeneratorArguments<HttpResponseMessage> args)
        {
            if (args.Outcome.Result?.StatusCode == (HttpStatusCode)429)
            {
                var rateLimitDelay = GetRetryAfterDelay(
                    args.Outcome.Result.Headers.RetryAfter,
                    TimeSpan.FromMilliseconds(_options.RetryDelayMs));
                return new ValueTask<TimeSpan?>(rateLimitDelay);
            }

            TimeSpan delay = TimeSpan.FromMilliseconds(_options.RetryDelayMs);
            if (_options.UseExponentialBackoff)
            {
                delay = TimeSpan.FromMilliseconds(
                    Math.Min(Math.Pow(2, args.AttemptNumber) * _options.RetryDelayMs, _options.MaxRetryDelayMs));
            }
#if NET6_0_OR_GREATER
            delay += TimeSpan.FromMilliseconds(Random.Shared.Next(0, (int)(delay.TotalMilliseconds * 0.2)));
#else
            delay += TimeSpan.FromMilliseconds(new Random().Next(0, (int)(delay.TotalMilliseconds * 0.2)));
#endif
            return new ValueTask<TimeSpan?>(delay);
        }

        private ValueTask OnRetryAttempt(OnRetryArguments<HttpResponseMessage> args)
        {
            string urlForLog = args.Context.Properties.TryGetValue(RelativeUrlKey, out var u) ? u : "Unknown URL";
            bool is429 = args.Outcome.Result?.StatusCode == (HttpStatusCode)429;

            EnkaTelemetry.RetryCount.Add(1);

            if (is429)
            {
                _logger.LogWarning(EnkaEventIds.RetryAttempt,
                    "Request to {Url} rate-limited (429). Waiting {Delay} (Retry-After), then retry {Attempt}/{Max}.",
                    urlForLog, args.RetryDelay, args.AttemptNumber + 1, _options.MaxRetries);
            }
            else
            {
                _logger.LogWarning(EnkaEventIds.RetryAttempt, args.Outcome.Exception,
                    "Request to {Url} failed. Delaying {Delay}, then retry {Attempt}/{Max}. Status: {StatusCode}",
                    urlForLog, args.RetryDelay, args.AttemptNumber + 1, _options.MaxRetries,
                    args.Outcome.Result?.StatusCode);
            }
            return new ValueTask();
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
                    _logger.LogTrace(EnkaEventIds.CacheHit, "Cache hit for {Url}", relativeUrl);
                    EnkaTelemetry.CacheHits.Add(1);
                    _trackedCacheKeys.TryAdd(cacheKey, true);
#if NET8_0_OR_GREATER
                    return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse, EnkaJsonContext.Default.Options)
                        ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} (result was null)");
#else
#pragma warning disable IL2026, IL3050
                    return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse)
                        ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} (result was null)");
#pragma warning restore IL2026, IL3050
#endif
                }
                EnkaTelemetry.CacheMisses.Add(1);
                _logger.LogTrace(EnkaEventIds.CacheMiss, "Cache miss for {Url}", relativeUrl);
            }

            HttpResponseMessage? response = null;
            string? jsonString = null;
            ResilienceContext? resilienceContext = null;
            var sw = Stopwatch.StartNew();

            using var activity = EnkaTelemetry.ActivitySource.StartActivity("EnkaHttp.Get");
            activity?.SetTag("enka.url", relativeUrl);

            EnkaTelemetry.RequestCount.Add(1);
            _logger.LogTrace(EnkaEventIds.RequestStart, "Sending request to {Url}", relativeUrl);

            try
            {
                resilienceContext = ResilienceContextPool.Shared.Get(cancellationToken);
                resilienceContext.Properties.Set(RelativeUrlKey, relativeUrl);

                response = await _resiliencePipeline.ExecuteAsync(
                    async (ctx, ct) =>
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, ctx.Properties.GetValue(RelativeUrlKey, "ERROR_NO_URL_IN_CONTEXT"));
                        HttpResponseMessage httpResponse = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

                        if (httpResponse.StatusCode == (HttpStatusCode)424)
                            throw new GameMaintenanceException($"API returned 424 Failed Dependency for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}.");

                        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                            throw new PlayerNotFoundException(ExtractUidFromUrl(ctx.Properties.GetValue(RelativeUrlKey, "")),
                                $"API returned 404 for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}");

                        if (httpResponse.StatusCode == HttpStatusCode.Forbidden)
                            throw new ProfilePrivateException(ExtractUidFromUrl(ctx.Properties.GetValue(RelativeUrlKey, "")),
                                $"API returned 403 for URL: {ctx.Properties.GetValue(RelativeUrlKey, "")}. Profile may be private.");

                        return httpResponse;
                    },
                    resilienceContext
                ).ConfigureAwait(false);

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    _logger.LogWarning(EnkaEventIds.RateLimited,
                        "Rate limit (429) persisted after exhausting retries for {Url}. Retry-After: {RetryAfter}",
                        relativeUrl, response.Headers.RetryAfter);
                    activity?.SetStatus(ActivityStatusCode.Error, "RateLimited");
                    throw new RateLimitException(
                        $"API rate limit exceeded for URL: {relativeUrl}. Retry-After: {response.Headers.RetryAfter}",
                        response.Headers.RetryAfter);
                }

                cancellationToken.ThrowIfCancellationRequested();
                response.EnsureSuccessStatusCode();

                jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(jsonString))
                    _logger.LogWarning("Received empty or whitespace JSON response from {Url}", relativeUrl);

                cancellationToken.ThrowIfCancellationRequested();

#if NET8_0_OR_GREATER
                T? deserializedObject = JsonSerializer.Deserialize<T>(jsonString, EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                T? deserializedObject = JsonSerializer.Deserialize<T>(jsonString);
#pragma warning restore IL2026, IL3050
#endif
                if (deserializedObject == null && !string.IsNullOrWhiteSpace(jsonString))
                    throw new EnkaNetworkException($"Failed to deserialize JSON response from {relativeUrl}, but content was not empty.");

                if (response.IsSuccessStatusCode && _options.EnableCaching)
                    _ = CacheSuccessfulResponse(cacheKey, jsonString, response, relativeUrl);

                activity?.SetTag("enka.status_code", (int)response.StatusCode);
                return deserializedObject!;
            }
            catch (JsonException ex)
            {
                string snippet = jsonString?.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString ?? "null";
                _logger.LogError(ex, "Failed to parse JSON from {Url}. Snippet: {Snippet}", relativeUrl, snippet);
                activity?.SetStatus(ActivityStatusCode.Error, "JsonParseFailed");
                throw new EnkaNetworkException($"Failed to parse JSON response from {relativeUrl}. Snippet: {snippet}", ex);
            }
            finally
            {
                sw.Stop();
                EnkaTelemetry.RequestDurationMs.Record(sw.Elapsed.TotalMilliseconds);
                response?.Dispose();
                if (resilienceContext != null)
                    ResilienceContextPool.Shared.Return(resilienceContext);
            }
        }

        private static TimeSpan GetRetryAfterDelay(RetryConditionHeaderValue? retryAfterHeader, TimeSpan defaultDelay)
        {
            if (retryAfterHeader != null)
            {
                if (retryAfterHeader.Delta.HasValue)
                    return retryAfterHeader.Delta.Value > TimeSpan.Zero ? retryAfterHeader.Delta.Value : defaultDelay;
                if (retryAfterHeader.Date.HasValue)
                {
                    var delay = retryAfterHeader.Date.Value - DateTimeOffset.UtcNow;
                    return delay > TimeSpan.Zero ? delay : defaultDelay;
                }
            }
            return defaultDelay;
        }

        private async Task CacheSuccessfulResponse(string cacheKey, string jsonString, HttpResponseMessage response, string relativeUrl)
        {
            DateTimeOffset expiration = CalculateExpiration(response, _options.CacheDurationMinutes);
            var newCacheEntry = new CacheEntry { JsonResponse = jsonString, Expiration = expiration };
            var ttl = expiration - DateTimeOffset.UtcNow;
            if (ttl > TimeSpan.Zero)
            {
                try
                {
                    await _cache.SetAsync(cacheKey, newCacheEntry, ttl).ConfigureAwait(false);
                    _trackedCacheKeys.TryAdd(cacheKey, true);
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
                return DateTimeOffset.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
            if (response.Content?.Headers?.Expires != null)
            {
                var expiresHeader = response.Content.Headers.Expires.Value;
                if (expiresHeader > DateTimeOffset.UtcNow) return expiresHeader;
            }
            return DateTimeOffset.UtcNow.AddMinutes(defaultCacheDurationMinutes);
        }

        public void ClearCache() => ClearCacheAsync().GetAwaiter().GetResult();

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
                foreach (var key in _trackedCacheKeys.Keys)
                {
                    try { await _cache.RemoveAsync(key, cancellationToken).ConfigureAwait(false); }
                    catch (Exception removeEx) { _logger.LogTrace(removeEx, "Skipping failed per-key cache removal for {Key}.", key); }
                }
                _trackedCacheKeys.Clear();
            }
        }

        public void RemoveFromCache(string relativeUrl) => RemoveFromCacheAsync(relativeUrl).GetAwaiter().GetResult();

        public async Task RemoveFromCacheAsync(string relativeUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            string cacheKey = relativeUrl.ToLowerInvariant();
            try
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);
                _trackedCacheKeys.TryRemove(cacheKey, out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove {CacheKey} from cache.", cacheKey);
            }
        }

        public (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats() =>
            GetCacheStatsAsync().GetAwaiter().GetResult();

        public async Task<(long CurrentEntryCount, int ExpiredCountNotAvailable)> GetCacheStatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var stats = await _cache.GetStatsAsync(cancellationToken).ConfigureAwait(false);
                return (stats.EntryCount, 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get cache stats, returning tracked keys count.");
                return (_trackedCacheKeys.Count, 0);
            }
        }

        public async Task<CacheStatistics> GetDetailedCacheStatsAsync(CancellationToken cancellationToken = default) =>
            await _cache.GetStatsAsync(cancellationToken).ConfigureAwait(false);

        private int ExtractUidFromUrl(string url)
        {
            var parts = url?.Split('/');
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Equals("uid", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length && int.TryParse(parts[i + 1], out int uid1)) return uid1;
                    if (i == parts.Length - 1 && int.TryParse(parts[i], out int uid2)) return uid2;
                    if (i == 0 && int.TryParse(parts[i], out int uid3)) return uid3;
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
                    if (_legacyMemoryCache == null)
                        _cache?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
