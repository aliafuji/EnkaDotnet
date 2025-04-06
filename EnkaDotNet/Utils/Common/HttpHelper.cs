using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils;
using EnkaDotNet.Exceptions;

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
            
            // Set base address if it's a complete URL
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
            else
            {
                // Otherwise fallback to default with relative path appended
                client.BaseAddress = new Uri(Constants.GetBaseUrl(gameType) + baseUrl);
            }
            
            client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(0),
                NoCache = !options.EnableCaching,
                MustRevalidate = options.EnableCaching
            };

            return client;
        }

        private bool IsEmptyOrInvalidProfile<T>(string jsonString, T result) where T : class
        {
            if (typeof(T) != typeof(ApiResponse))
                return false;

            bool hasMinimalPlayerInfo = jsonString.Contains("\"playerInfo\":{") &&
                                       !jsonString.Contains("\"nickname\":");

            bool hasNoAvatarList = !jsonString.Contains("\"avatarInfoList\":");

            return hasMinimalPlayerInfo && hasNoAvatarList;
        }

        public async Task<T> Get<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HttpHelper));
            
            string cacheKey = _cache.GenerateCacheKey(relativeUrl);

            if (_options.EnableCaching && !bypassCache && _cache.TryGetValue(cacheKey, out CacheEntry? cacheEntry) && !cacheEntry.IsExpired)
            {
                LogVerbose($"Cache hit for {relativeUrl}");
                return JsonSerializer.Deserialize<T>(cacheEntry.JsonResponse) ??
                    throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl}");
            }

            int attempts = 0;
            while (true)
            {
                attempts++;
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

                    if (_options.EnableCaching && !bypassCache && _cache.TryGetValue(cacheKey, out cacheEntry) && !string.IsNullOrEmpty(cacheEntry.ETag))
                    {
                        request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cacheEntry.ETag));
                    }

                    LogVerbose($"Sending request to {relativeUrl}");
                    HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotModified &&
                        _options.EnableCaching && !bypassCache && _cache.TryGetValue(cacheKey, out cacheEntry))
                    {
                        LogVerbose($"304 Not Modified for {relativeUrl}, using cached data");
                        _cache.UpdateCacheEntryExpiration(cacheKey, cacheEntry, response);
                        return JsonSerializer.Deserialize<T>(cacheEntry.JsonResponse) ??
                            throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl}");
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        int uid = ExtractUidFromUrl(relativeUrl);
                        throw new PlayerNotFoundException(uid, $"API returned 404 Not Found for URL: {relativeUrl}");
                    }
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        int uid = ExtractUidFromUrl(relativeUrl);
                        throw new ProfilePrivateException(uid, $"API returned 403 Forbidden for URL: {relativeUrl}. Profile may be private.");
                    }

                    response.EnsureSuccessStatusCode();

                    string jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
                    LogVerbose($"Received {jsonString.Length} bytes from {relativeUrl}");

                    if (response.IsSuccessStatusCode && _options.EnableCaching)
                    {
                        _cache.StoreResponse(cacheKey, jsonString, response);
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = false,
                    };

                    T? result = JsonSerializer.Deserialize<T>(jsonString, options);
                    if (result == null)
                    {
                        throw new EnkaNetworkException($"Failed to deserialize JSON response from {relativeUrl}. Response was null.");
                    }

                    if (IsEmptyOrInvalidProfile(jsonString, result))
                    {
                        int uid = ExtractUidFromUrl(relativeUrl);
                        throw new PlayerNotFoundException(uid, $"Profile appears to be invalid or empty: UID {uid}");
                    }

                    return result;
                }
                catch (HttpRequestException ex) when (attempts <= _options.MaxRetries)
                {
                    LogVerbose($"Request failed (attempt {attempts}/{_options.MaxRetries + 1}): {ex.Message}");
                    if (attempts <= _options.MaxRetries)
                    {
                        await Task.Delay(_options.RetryDelayMs, cancellationToken);
                        continue;
                    }
                    throw new EnkaNetworkException($"HTTP request failed for URL: {relativeUrl} after {attempts} attempts. Status: {ex.StatusCode}", ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new EnkaNetworkException($"HTTP request failed for URL: {relativeUrl}. Status: {ex.StatusCode}", ex);
                }
                catch (JsonException ex)
                {
                    throw new EnkaNetworkException($"Failed to parse JSON response from {relativeUrl}.", ex);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    throw new EnkaNetworkException($"Request timed out for URL: {relativeUrl}.", ex);
                }
                catch (TaskCanceledException ex)
                {
                    throw new OperationCanceledException($"Request was canceled for URL: {relativeUrl}.", ex);
                }
            }
        }

        public void ClearCache() => _cache.Clear();

        public void RemoveFromCache(string relativeUrl) => _cache.Remove(_cache.GenerateCacheKey(relativeUrl));

        public (int Count, int ExpiredCount) GetCacheStats() => _cache.GetStats();

        private int ExtractUidFromUrl(string url)
        {
            var parts = url?.Split('/');
            if (parts?.Length >= 2 && int.TryParse(parts[1], out int uid))
            {
                return uid;
            }
            return 0;
        }

        private void LogVerbose(string message)
        {
            if (_options.EnableVerboseLogging)
            {
                Console.WriteLine($"[HttpHelper] {message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}