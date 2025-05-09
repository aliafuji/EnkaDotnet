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
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;
using System.Collections.Concurrent;
using System.Linq;

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
        }

        private bool IsEmptyOrInvalidProfile<T>(string jsonString) where T : class
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return true;
            }

            if (typeof(T) == typeof(ApiResponse))
            {
                bool hasPlayerInfoBlock = jsonString.Contains("\"playerInfo\":{");
                bool lacksNickname = !jsonString.Contains("\"nickname\":");
                bool hasMinimalPlayerInfo = hasPlayerInfoBlock && lacksNickname;

                bool hasNoAvatarList = !jsonString.Contains("\"avatarInfoList\":") || jsonString.Contains("\"avatarInfoList\":[]");

                if (hasMinimalPlayerInfo && hasNoAvatarList)
                {
                    _logger.LogWarning("Genshin profile appears empty/invalid based on content: {JsonSnippet}", jsonString.Substring(0, Math.Min(jsonString.Length, 200)));
                    return true;
                }
            }
            else if (typeof(T) == typeof(HSRApiResponse))
            {
                bool hasDetailInfoBlock = jsonString.Contains("\"detailInfo\":{");
                bool lacksNicknameInDetailInfo = hasDetailInfoBlock && !jsonString.Contains("\"nickname\":");

                bool hasNoAvatarDetailList = !jsonString.Contains("\"avatarDetailList\":") || jsonString.Contains("\"avatarDetailList\":[]");

                if (hasDetailInfoBlock && lacksNicknameInDetailInfo && hasNoAvatarDetailList)
                {
                    _logger.LogWarning("HSR profile appears empty/invalid based on content: {JsonSnippet}", jsonString.Substring(0, Math.Min(jsonString.Length, 200)));
                    return true;
                }
                if (!hasDetailInfoBlock && !jsonString.Contains("\"detailInfo\":null"))
                {
                    if (jsonString == "{}") return true;
                }
            }
            else if (typeof(T) == typeof(ZZZApiResponse))
            {
                bool hasPlayerInfoBlock = jsonString.Contains("\"PlayerInfo\":{");
                bool lacksProfileDetailNickname = hasPlayerInfoBlock && !jsonString.Contains("\"Nickname\":\"");

                bool hasNoAvatarList = !jsonString.Contains("\"AvatarList\":") || jsonString.Contains("\"AvatarList\":[]");

                if (hasPlayerInfoBlock && lacksProfileDetailNickname && hasNoAvatarList)
                {
                    _logger.LogWarning("ZZZ profile appears empty/invalid based on content: {JsonSnippet}", jsonString.Substring(0, Math.Min(jsonString.Length, 200)));
                    return true;
                }
            }

            return false;
        }

        public async Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpHelper));

            string cacheKey = relativeUrl.ToLowerInvariant();
            CacheEntry cachedEntry = null;

            if (_options.EnableCaching && !bypassCache && _memoryCache.TryGetValue(cacheKey, out cachedEntry) && cachedEntry != null)
            {
                _logger.Log(LogLevel.Trace, "Cache hit for {Url}", relativeUrl);
                _trackedCacheKeys.TryAdd(cacheKey, true);
            }
            else
            {
                cachedEntry = null;
            }

            int attempts = 0;
            while (true)
            {
                attempts++;
                HttpResponseMessage response = null;
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl))
                    {
                        if (_options.EnableCaching && !bypassCache && cachedEntry != null && !string.IsNullOrEmpty(cachedEntry.ETag))
                        {
                            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cachedEntry.ETag));
                        }

                        _logger.Log(LogLevel.Trace, "Attempt {Attempt}: Sending request to {BaseAddress}{Url}", attempts, _httpClient.BaseAddress, relativeUrl);
                        response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                        if (response.StatusCode == HttpStatusCode.NotModified &&
                            _options.EnableCaching && !bypassCache && cachedEntry != null)
                        {
                            _logger.Log(LogLevel.Trace, "304 Not Modified for {Url}, using cached data and updating expiration.", relativeUrl);
                            var newExpiration = CalculateExpiration(response, _options.CacheDurationMinutes);
                            cachedEntry.Expiration = newExpiration.UtcDateTime;
                            var updatedEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(newExpiration);
                            _memoryCache.Set(cacheKey, cachedEntry, updatedEntryOptions);
                            _trackedCacheKeys.TryAdd(cacheKey, true);
                            return JsonSerializer.Deserialize<T>(cachedEntry.JsonResponse) ?? throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} after 304 (result was null)");
                        }

                        if (_options.RetryOnStatusCodes.Contains(response.StatusCode) && attempts <= _options.MaxRetries)
                        {
                            throw new HttpRequestException($"Response status code {response.StatusCode} indicates server error, triggering retry.");
                        }

                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new PlayerNotFoundException(uid, $"API returned 404 Not Found for URL: {relativeUrl}");
                        }
                        if (response.StatusCode == (HttpStatusCode)429)
                        {
                            RetryConditionHeaderValue retryAfter = response.Headers.RetryAfter;
                            _logger.LogWarning("API returned 429 Too Many Requests for URL: {Url}. Retry-After: {RetryAfter}", relativeUrl, retryAfter);
                            throw new RateLimitException($"API rate limit exceeded for URL: {relativeUrl}. Please try again later.", retryAfter);
                        }
                        if (response.StatusCode == HttpStatusCode.InternalServerError && !_options.RetryOnStatusCodes.Contains(HttpStatusCode.InternalServerError))
                        {
                            string errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new EnkaNetworkException($"API returned 500 Internal Server Error for URL: {relativeUrl}. Response: {errorBody}");
                        }
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new ProfilePrivateException(uid, $"API returned 403 Forbidden for URL: {relativeUrl}. Profile may be private or API access restricted.");
                        }

                        response.EnsureSuccessStatusCode();

                        string jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        _logger.Log(LogLevel.Trace, "Received {ByteCount} bytes from {Url}", jsonString.Length, relativeUrl);

                        if (IsEmptyOrInvalidProfile<T>(jsonString))
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new PlayerNotFoundException(uid, $"Profile for UID {uid} appears to be invalid or empty based on content analysis, despite a successful HTTP status.");
                        }

                        if (response.IsSuccessStatusCode && _options.EnableCaching)
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

                        return JsonSerializer.Deserialize<T>(jsonString) ?? throw new EnkaNetworkException($"Failed to deserialize JSON response from {relativeUrl}. Result was null.");
                    }
                }
                catch (RateLimitException) { throw; }
                catch (ProfilePrivateException) { throw; }
                catch (PlayerNotFoundException) { throw; }
                catch (HttpRequestException ex) when (attempts <= _options.MaxRetries)
                {
                    _logger.LogWarning(ex, "Request failed (attempt {Attempt}/{MaxAttempts}): {ErrorMessage} for URL: {Url}. Status Code: {StatusCode}", attempts, _options.MaxRetries + 1, ex.Message, relativeUrl, response?.StatusCode);

                    int delayMs = _options.RetryDelayMs;
                    if (_options.UseExponentialBackoff)
                    {
                        delayMs = (int)Math.Min(_options.RetryDelayMs * Math.Pow(2, attempts - 1), _options.MaxRetryDelayMs);
                        delayMs += _jitterer.Next(0, (int)(delayMs * 0.1));
                    }

                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    cachedEntry = null;
                    continue;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "HTTP request failed for URL: {Url} after {Attempts} attempts. Status Code: {StatusCode}", relativeUrl, attempts, response?.StatusCode);
                    throw new EnkaNetworkException($"HTTP request failed for URL: {relativeUrl} after {attempts} attempts. Message: {ex.Message}", ex);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    string snippet = "Could not read response content for snippet.";
                    if (response?.Content != null)
                    {
                        try
                        {
                            snippet = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            snippet = snippet.Length > 200 ? snippet.Substring(0, 200) + "..." : snippet;
                        }
                        catch { }
                    }
                    _logger.LogError(ex, "Failed to parse JSON response from {Url}. Snippet: {Snippet}", relativeUrl, snippet);
                    throw new EnkaNetworkException($"Failed to parse JSON response from {relativeUrl}. Snippet: {snippet}", ex);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning(ex, "Request timed out for URL: {Url} on attempt {Attempt}", relativeUrl, attempts);
                    if (attempts <= _options.MaxRetries)
                    {
                        await Task.Delay(_options.RetryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    throw new EnkaNetworkException($"Request timed out for URL: {relativeUrl}.", ex);
                }
                catch (TaskCanceledException ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Request canceled by cancellation token for URL: {Url}", relativeUrl);
                        throw;
                    }
                    _logger.LogWarning(ex, "Request task was canceled, possibly due to timeout for URL: {Url} on attempt {Attempt}", relativeUrl, attempts);
                    if (attempts <= _options.MaxRetries)
                    {
                        await Task.Delay(_options.RetryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    throw new EnkaNetworkException($"Request task was canceled for URL: {relativeUrl}.", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while fetching data from {Url} on attempt {Attempt}", relativeUrl, attempts);
                    if (attempts <= _options.MaxRetries)
                    {
                        await Task.Delay(_options.RetryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    throw new EnkaNetworkException($"An unexpected error occurred while fetching data from {relativeUrl}.", ex);
                }
                finally
                {
                    response?.Dispose();
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