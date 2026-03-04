using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _fallbackDirectory;
        private readonly ConcurrentDictionary<string, object> _assetCache = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _assetFetchLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
        protected ConcurrentDictionary<string, string> _textMap;
        protected readonly ILogger _logger;
        private volatile bool _isInitialized = false;
        private bool _disposed = false;

        public string Language { get; }
        public string GameIdentifier { get; }

        protected BaseAssets(string language, string gameIdentifier, HttpClient httpClient, ILogger logger, string fallbackDirectory = null)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            GameIdentifier = gameIdentifier ?? throw new ArgumentNullException(nameof(gameIdentifier));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? NullLogger.Instance;
            _fallbackDirectory = fallbackDirectory;
        }

        public async Task EnsureInitializedAsync()
        {
            if (_isInitialized) return;

            await _initializationSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_isInitialized) return;

                var timeout = TimeSpan.FromMinutes(5);
                var initializationTask = Task.WhenAll(
                    LoadTextMapInternalAsync(Language),
                    LoadAssetsInternalAsync()
                );

                var completedTask = await Task.WhenAny(initializationTask, Task.Delay(timeout)).ConfigureAwait(false);

                if (completedTask == initializationTask)
                {
                    await initializationTask.ConfigureAwait(false);
                }
                else
                {
                    throw new TimeoutException($"Asset initialization for {GameIdentifier} timed out after {timeout.TotalMinutes} minutes.");
                }

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

                    if (language != "en" && !allLanguageMaps.ContainsKey("en"))
                    {
                        string firstAvailableLanguage = null;
                        using (var enumerator = allLanguageMaps.Keys.GetEnumerator())
                        {
                            if (enumerator.MoveNext())
                            {
                                firstAvailableLanguage = enumerator.Current;
                            }
                        }

                        if (firstAvailableLanguage == null)
                        {
                            throw new InvalidOperationException($"No languages found in ZZZ TextMap data for {this.GameIdentifier}");
                        }
                        fallbackLanguage = firstAvailableLanguage;
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
                _logger.LogWarning("GetText called before assets for {GameIdentifier} were initialized. Call EnsureInitializedAsync() first. Returning hash or empty string as fallback.", GameIdentifier);
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
                string json;
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                    HttpResponseMessage response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                if (_fallbackDirectory != null)
                    _ = SaveAssetToDiskAsync(assetKey, json);

                return json;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                if (_fallbackDirectory != null)
                {
                    string localPath = GetFallbackPath(assetKey);
                    if (File.Exists(localPath))
                    {
                        _logger.LogWarning(ex,
                            "Network error fetching '{AssetKey}' for {GameIdentifier}. Loading from local fallback: {Path}",
                            assetKey, GameIdentifier, localPath);
#if NET8_0_OR_GREATER
                        return await File.ReadAllTextAsync(localPath).ConfigureAwait(false);
#else
                        return File.ReadAllText(localPath);
#endif
                    }
                }

                throw;
            }
        }

        private string GetFallbackPath(string assetKey)
        {
            return Path.Combine(_fallbackDirectory, GameIdentifier, assetKey);
        }

        private async Task SaveAssetToDiskAsync(string assetKey, string json)
        {
            try
            {
                string dir = Path.Combine(_fallbackDirectory, GameIdentifier);
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, assetKey);
#if NET8_0_OR_GREATER
                await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
#else
                File.WriteAllText(path, json);
                await Task.CompletedTask.ConfigureAwait(false);
#endif
                _logger.LogTrace("Asset '{AssetKey}' for {GameIdentifier} saved to fallback: {Path}", assetKey, GameIdentifier, path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save asset '{AssetKey}' for {GameIdentifier} to fallback directory.", assetKey, GameIdentifier);
            }
        }

        protected async Task<T> FetchAndDeserializeAssetAsync<T>(string assetKey) where T : class
        {
            string cacheKey = $"{GameIdentifier}_{assetKey}";

            if (_assetCache.TryGetValue(cacheKey, out var cachedAsset) && cachedAsset is T typedAsset)
            {
                return typedAsset;
            }

            var fetchLock = _assetFetchLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

            await fetchLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_assetCache.TryGetValue(cacheKey, out cachedAsset) && cachedAsset is T recheckedAsset)
                {
                    return recheckedAsset;
                }

                string jsonContent = await FetchAssetAsync(assetKey).ConfigureAwait(false);
                try
                {
#if NET8_0_OR_GREATER
                    var result = JsonSerializer.Deserialize<T>(jsonContent, EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                    var result = JsonSerializer.Deserialize<T>(jsonContent);
#pragma warning restore IL2026, IL3050
#endif
                    if (result == null)
                    {
                        throw new JsonException($"Failed to deserialize {assetKey} for {GameIdentifier} - result was null.");
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
            finally
            {
                fetchLock.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _initializationSemaphore.Dispose();
                foreach (var semaphore in _assetFetchLocks.Values)
                {
                    semaphore.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
