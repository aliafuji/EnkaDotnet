using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets
    {
        private readonly HttpClient _httpClient;
        protected readonly Dictionary<string, object> _assetCache = new Dictionary<string, object>();
        private static readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
        protected Dictionary<string, string> _textMap;
        protected readonly ILogger _logger;
        private volatile bool _isInitialized = false;

        public GameType GameType { get; }
        public string Language { get; }

        protected BaseAssets(string language, GameType gameType, HttpClient httpClient, ILogger logger)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            GameType = gameType;
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

        protected virtual async Task LoadTextMapInternalAsync(string language)
        {
            try
            {
                _textMap = new Dictionary<string, string>();
                var allLanguageMaps = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json").ConfigureAwait(false);

                if (allLanguageMaps.TryGetValue(language, out var languageSpecificMap))
                {
                    _textMap = languageSpecificMap;
                }
                else
                {
                    _logger.LogWarning("Language code '{Language}' not found in the TextMap file for {GameType}. Available: {AvailableLanguages}", language, this.GameType, string.Join(", ", allLanguageMaps.Keys));
                    string fallbackLanguage = "en";
                    if (this.GameType == GameType.ZZZ && language != "en" && !allLanguageMaps.ContainsKey("en"))
                    {
                        fallbackLanguage = System.Linq.Enumerable.FirstOrDefault(allLanguageMaps.Keys);
                        if (fallbackLanguage == null) throw new InvalidOperationException($"No languages found in ZZZ TextMap data.");
                    }

                    if (allLanguageMaps.TryGetValue(fallbackLanguage, out var fallbackMap))
                    {
                        _logger.LogInformation("Falling back to '{FallbackLanguage}' language for {GameType}.", fallbackLanguage, this.GameType);
                        _textMap = fallbackMap;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Fallback language '{fallbackLanguage}' also not found for {GameType}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TextMap for {GameType}.", this.GameType);
                throw new InvalidOperationException($"Failed to load essential TextMap for {this.GameType}", ex);
            }
        }

        public string GetText(string hash)
        {
            if (!_isInitialized)
            {
                _logger.LogError("GetText called before assets were initialized. Call EnsureInitializedAsync() first. Returning hash or empty string as fallback.");
                return hash ?? string.Empty;
            }
            return hash != null && _textMap != null && _textMap.TryGetValue(hash, out var text) ? text ?? string.Empty : hash ?? string.Empty;
        }

        protected async Task<string> FetchAssetAsync(string assetKey)
        {
            var assetFileUrls = GetAssetFileUrls();
            if (!assetFileUrls.TryGetValue(assetKey, out var url))
            {
                _logger.LogError("No URL defined for asset '{AssetKey}' in game type {GameType}.", assetKey, GameType);
                throw new InvalidOperationException($"No URL defined for asset '{assetKey}' in game type {GameType}.");
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
                _logger.LogError(ex, "FAILED to fetch '{AssetKey}' (Network Error). URL: {Url}", assetKey, url);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching '{AssetKey}'. URL: {Url}", assetKey, url);
                throw;
            }
        }

        protected async Task<T> FetchAndDeserializeAssetAsync<T>(string assetKey)
        {
            if (_assetCache.TryGetValue(assetKey, out var cachedAsset) && cachedAsset is T typedAsset)
            {
                return typedAsset;
            }

            string jsonContent = await FetchAssetAsync(assetKey).ConfigureAwait(false);
            try
            {
                var result = JsonConvert.DeserializeObject<T>(jsonContent);
                if (result == null)
                {
                    throw new JsonException($"Failed to deserialize {assetKey} - result was null");
                }
                _assetCache[assetKey] = result;
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing {AssetKey} JSON.", assetKey);
                throw;
            }
        }

        protected virtual IReadOnlyDictionary<string, string> GetAssetFileUrls()
        {
            return Constants.GetGameAssetFileUrls(this.GameType);
        }
    }
}