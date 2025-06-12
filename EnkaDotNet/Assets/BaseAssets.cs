using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets
    {
        private readonly HttpClient _httpClient;
        protected readonly ConcurrentDictionary<string, object> _assetCache = new ConcurrentDictionary<string, object>();
        private static readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
        protected ConcurrentDictionary<string, string> _textMap;
        protected readonly ILogger _logger;
        private volatile bool _isInitialized = false;

        public string Language { get; }
        public string GameIdentifier { get; }

        protected BaseAssets(string language, string gameIdentifier, HttpClient httpClient, ILogger logger)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            GameIdentifier = gameIdentifier ?? throw new ArgumentNullException(nameof(gameIdentifier));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? NullLogger.Instance;
        }

        public async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;
            await _initializationSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_isInitialized) return;
                await LoadTextMapInternalAsync(Language).ConfigureAwait(false);
                await LoadAssetsInternalAsync().ConfigureAwait(false);
                _isInitialized = true;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        protected abstract Task LoadAssetsInternalAsync();
        protected abstract IReadOnlyDictionary<string, string> GetAssetFileUrls();

        protected virtual async Task LoadTextMapInternalAsync(string language)
        {
            try
            {
                var allLanguageMaps = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json").ConfigureAwait(false);

                if (allLanguageMaps.TryGetValue(language, out var languageSpecificMap))
                {
                    _textMap = new ConcurrentDictionary<string, string>(languageSpecificMap);
                }
                else
                {
                    _logger.LogWarning("Language code '{Language}' not found in the TextMap file for {GameIdentifier}. Available: {AvailableLanguages}", language, this.GameIdentifier, string.Join(", ", allLanguageMaps.Keys));
                    string fallbackLanguage = "en";

                    if (this.GameIdentifier == "zzz" && language != "en" && !allLanguageMaps.ContainsKey("en"))
                    {
                        fallbackLanguage = System.Linq.Enumerable.FirstOrDefault(allLanguageMaps.Keys);
                        if (fallbackLanguage == null) throw new InvalidOperationException($"No languages found in ZZZ TextMap data for {this.GameIdentifier}");
                    }

                    if (allLanguageMaps.TryGetValue(fallbackLanguage, out var fallbackMap))
                    {
                        _logger.LogInformation("Falling back to '{FallbackLanguage}' language for {GameIdentifier}", fallbackLanguage, this.GameIdentifier);
                        _textMap = new ConcurrentDictionary<string, string>(fallbackMap);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Fallback language '{fallbackLanguage}' also not found for {this.GameIdentifier}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TextMap for {GameIdentifier}", this.GameIdentifier);
                throw new InvalidOperationException($"Failed to load essential TextMap for {this.GameIdentifier}", ex);
            }
        }

        public string GetText(string hash)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("GetText called before assets for {GameIdentifier} were initialized Call EnsureInitializedAsync() first Returning hash or empty string as fallback", GameIdentifier);
                return hash ?? string.Empty;
            }
            return hash != null && _textMap != null && _textMap.TryGetValue(hash, out var text) ? text ?? string.Empty : hash ?? string.Empty;
        }

        protected async Task<string> FetchAssetAsync(string assetKey)
        {
            var assetFileUrls = GetAssetFileUrls();
            if (!assetFileUrls.TryGetValue(assetKey, out var url))
            {
                _logger.LogError("No URL defined for asset '{AssetKey}' in game {GameIdentifier}", assetKey, GameIdentifier);
                throw new InvalidOperationException($"No URL defined for asset '{assetKey}' in game {GameIdentifier}");
            }
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                    HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "FAILED to fetch '{AssetKey}' for {GameIdentifier} (Network Error) URL: {Url}", assetKey, GameIdentifier, url);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching '{AssetKey}' for {GameIdentifier} URL: {Url}", assetKey, GameIdentifier, url);
                throw;
            }
        }

        protected async Task<T> FetchAndDeserializeAssetAsync<T>(string assetKey)
        {
            string cacheKey = $"{GameIdentifier}_{assetKey}";
            if (_assetCache.TryGetValue(cacheKey, out var cachedAsset) && cachedAsset is T typedAsset)
            {
                return typedAsset;
            }

            string jsonContent = await FetchAssetAsync(assetKey).ConfigureAwait(false);
            try
            {
                var result = JsonSerializer.Deserialize<T>(jsonContent);
                if (result == null)
                {
                    throw new JsonException($"Failed to deserialize {assetKey} for {GameIdentifier} - result was null");
                }
                _assetCache[cacheKey] = result;
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing {AssetKey} JSON for {GameIdentifier}", assetKey, GameIdentifier);
                throw;
            }
        }
    }
}