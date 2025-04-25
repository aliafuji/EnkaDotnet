using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using Newtonsoft.Json;
using EnkaDotNet.Models.Genshin;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace EnkaDotNet.Utils.Common
{
    public class HttpHelper : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly HttpCache _cache;
        private readonly GameType _gameType;
        private readonly EnkaClientOptions _options;
        private bool _disposed = false;

        public HttpHelper(GameType gameType, EnkaClientOptions options)
        {
            if (!Constants.IsGameTypeSupported(gameType))
            {
                throw new NotSupportedException($"Game type {gameType} is not supported.");
            }

            _gameType = gameType;
            _options = options ?? new EnkaClientOptions();
            _cache = new HttpCache(_options.CacheDurationMinutes);
            _httpClient = CreateHttpClient(gameType, _options);
        }

        private static HttpClient CreateHttpClient(GameType gameType, EnkaClientOptions options)
        {
            string userAgent = options.UserAgent ?? Constants.DefaultUserAgent;
            string baseUrl = options.BaseUrl ?? Constants.GetBaseUrl(gameType);

            var client = new HttpClient();

            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
            else
            {
                client.BaseAddress = new Uri(new Uri(Constants.GetBaseUrl(gameType)), baseUrl);
            }

            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = !options.EnableCaching,
                NoStore = !options.EnableCaching
            };

            return client;
        }

        private bool IsEmptyOrInvalidProfile<T>(string jsonString) where T : class
        {
            if (typeof(T) == typeof(ApiResponse))
            {
                bool hasMinimalPlayerInfo = jsonString.Contains("\"playerInfo\":{") &&
                                           !jsonString.Contains("\"nickname\":");
                bool hasNoAvatarList = !jsonString.Contains("\"avatarInfoList\":") || jsonString.Contains("\"avatarInfoList\":[]");
                return hasMinimalPlayerInfo && hasNoAvatarList;
            }
            return false;
        }

        public async Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpHelper));

            string cacheKey = _cache.GenerateCacheKey(relativeUrl);
            CacheEntry cacheEntry = null;

            if (_options.EnableCaching && !bypassCache && _cache.TryGetValue(cacheKey, out cacheEntry) && !cacheEntry.IsExpired)
            {
                LogVerbose($"Cache hit for {relativeUrl}");
                try
                {
                    return JsonConvert.DeserializeObject<T>(cacheEntry.JsonResponse) ??
                          throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} (result was null)");
                }
                catch (JsonException ex)
                {
                    throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl}", ex);
                }
            }

            int attempts = 0;
            while (true)
            {
                attempts++;
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl))
                    {
                        if (_options.EnableCaching && !bypassCache && cacheEntry != null && !string.IsNullOrEmpty(cacheEntry.ETag))
                        {
                            request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cacheEntry.ETag));
                            LogVerbose($"Using ETag: {cacheEntry.ETag} for {relativeUrl}");
                        }
                        else
                        {
                            LogVerbose($"No valid ETag found or cache bypassed for {relativeUrl}");
                        }

                        LogVerbose($"Sending request to {relativeUrl}");
                        HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                        if (response.StatusCode == System.Net.HttpStatusCode.NotModified &&
                            _options.EnableCaching && !bypassCache && cacheEntry != null)
                        {
                            LogVerbose($"304 Not Modified for {relativeUrl}, using cached data");
                            _cache.UpdateCacheEntryExpiration(cacheKey, cacheEntry, response);
                            try
                            {
                                return JsonConvert.DeserializeObject<T>(cacheEntry.JsonResponse) ??
                                     throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} after 304 (result was null)");
                            }
                            catch (JsonException ex)
                            {
                                throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl} after 304", ex);
                            }
                        }

                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new PlayerNotFoundException(uid, $"API returned 404 Not Found for URL: {relativeUrl}");
                        }
                        if ((int)response.StatusCode == 429)
                        {
                            throw new EnkaNetworkException($"API returned 429 Too Many Requests for URL: {relativeUrl}. Please try again later.");
                        }
                        if ((int)response.StatusCode == 500)
                        {
                            string errorBody = await response.Content.ReadAsStringAsync();
                            throw new EnkaNetworkException($"API returned 500 Internal Server Error for URL: {relativeUrl}. Response: {errorBody}");
                        }
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new ProfilePrivateException(uid, $"API returned 403 Forbidden for URL: {relativeUrl}. Profile may be private or API access restricted.");
                        }

                        response.EnsureSuccessStatusCode();

                        string jsonString = await response.Content.ReadAsStringAsync();
                        LogVerbose($"Received {jsonString.Length} bytes from {relativeUrl}");

                        if (string.IsNullOrWhiteSpace(jsonString))
                        {
                            throw new EnkaNetworkException($"Received empty response from {relativeUrl}.");
                        }

                        if (IsEmptyOrInvalidProfile<T>(jsonString))
                        {
                            int uid = ExtractUidFromUrl(relativeUrl);
                            throw new PlayerNotFoundException(uid, $"Profile appears to be invalid or empty: UID {uid}");
                        }

                        if (response.IsSuccessStatusCode && _options.EnableCaching)
                        {
                            _cache.StoreResponse(cacheKey, jsonString, response);
                            LogVerbose($"Stored response in cache for {relativeUrl}");
                        }

                        try
                        {
                            T result = JsonConvert.DeserializeObject<T>(jsonString);
                            if (result == null)
                            {
                                throw new EnkaNetworkException($"Failed to deserialize JSON response from {relativeUrl}. Result was null.");
                            }
                            return result;
                        }
                        catch (JsonException ex)
                        {
                            string snippet = jsonString.Length > 200 ? jsonString.Substring(0, 200) + "..." : jsonString;
                            throw new EnkaNetworkException($"Failed to parse JSON response from {relativeUrl}. Snippet: {snippet}", ex);
                        }
                    }
                }
                catch (ProfilePrivateException)
                {
                    throw;
                }
                catch (PlayerNotFoundException)
                {
                    throw;
                }
                catch (HttpRequestException ex) when (attempts <= _options.MaxRetries)
                {
                    LogVerbose($"Request failed (attempt {attempts}/{_options.MaxRetries + 1}): {ex.Message}");
                    if (attempts <= _options.MaxRetries)
                    {
                        await Task.Delay(_options.RetryDelayMs, cancellationToken);
                        cacheEntry = null;
                        continue;
                    }
                    throw new EnkaNetworkException($"HTTP request failed for URL: {relativeUrl} after {attempts} attempts. Message: {ex.Message}", ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new EnkaNetworkException($"HTTP request failed for URL: {relativeUrl}. Message: {ex.Message}", ex);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    throw new EnkaNetworkException($"Request timed out for URL: {relativeUrl}.", ex);
                }
                catch (TaskCanceledException ex)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    else
                    {
                        throw new EnkaNetworkException($"Request task was canceled, possibly due to timeout for URL: {relativeUrl}.", ex);
                    }
                }
                catch (Exception ex)
                {
                    throw new EnkaNetworkException($"An unexpected error occurred while fetching data from {relativeUrl}.", ex);
                }
            }
        }

        public void ClearCache() => _cache.Clear();

        public void RemoveFromCache(string relativeUrl) => _cache.Remove(_cache.GenerateCacheKey(relativeUrl));

        public (int Count, int ExpiredCount) GetCacheStats() => _cache.GetStats();

        private int ExtractUidFromUrl(string url)
        {
            var parts = url?.Split('/');
            if (parts != null)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    int uidAfterSegment = 0;
                    int uidLastSegment = 0;
                    int uidFirstSegment = 0;

                    bool isUidSegment = parts[i].Equals("uid", StringComparison.OrdinalIgnoreCase);
                    bool hasNextPart = i + 1 < parts.Length;
                    bool nextPartIsInt = hasNextPart && int.TryParse(parts[i + 1], out uidAfterSegment);

                    bool isLastPart = i == parts.Length - 1;
                    bool lastPartIsInt = isLastPart && int.TryParse(parts[i], out uidLastSegment);

                    bool isFirstPart = i == 0;
                    bool firstPartIsInt = isFirstPart && int.TryParse(parts[i], out uidFirstSegment);


                    if (isUidSegment && nextPartIsInt)
                    {
                        return uidAfterSegment;
                    }
                    if (lastPartIsInt)
                    {
                        return uidLastSegment;
                    }
                    if (firstPartIsInt)
                    {
                        return uidFirstSegment;
                    }
                }
            }
            return 0;
        }

        private void LogVerbose(string message)
        {
            if (_options.EnableVerboseLogging)
            {
                Console.WriteLine($"[HttpHelper] {DateTime.Now:HH:mm:ss.fff} - {message}");
            }
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
                    _httpClient?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}