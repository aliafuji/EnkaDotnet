using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Exceptions;

namespace EnkaDotNet.Utils.Common
{
    public class HttpHelper
    {
        private readonly HttpClient _httpClient;
        private readonly HttpCache _cache = new HttpCache();
        private readonly GameType _gameType;

        public HttpHelper(GameType gameType, string? customUserAgent = null)
        {
            _gameType = gameType;
            _httpClient = CreateHttpClient(gameType, customUserAgent);
        }

        private static HttpClient CreateHttpClient(GameType gameType, string? customUserAgent = null)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Constants.GetBaseUrl(gameType));
            client.DefaultRequestHeaders.UserAgent.ParseAdd(customUserAgent ?? Constants.DefaultUserAgent);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                MaxAge = TimeSpan.FromSeconds(0),
                NoCache = false,
                MustRevalidate = true
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

        public async Task<T> GetAsync<T>(string relativeUrl, bool bypassCache = false, CancellationToken cancellationToken = default) where T : class
        {
            string cacheKey = _cache.GenerateCacheKey(relativeUrl);

            if (!bypassCache && _cache.TryGetValue(cacheKey, out CacheEntry? cacheEntry) && !cacheEntry.IsExpired)
            {
                return JsonSerializer.Deserialize<T>(cacheEntry.JsonResponse) ??
                    throw new EnkaNetworkException($"Failed to deserialize cached JSON response from {relativeUrl}");
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

                if (!bypassCache && _cache.TryGetValue(cacheKey, out cacheEntry) && !string.IsNullOrEmpty(cacheEntry.ETag))
                {
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(cacheEntry.ETag));
                }

                HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotModified &&
                    !bypassCache && _cache.TryGetValue(cacheKey, out cacheEntry))
                {
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

                if (response.IsSuccessStatusCode)
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
    }
}