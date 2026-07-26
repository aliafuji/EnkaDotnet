using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#if NET8_0_OR_GREATER
using EnkaDotNet.Serialization;
#endif

namespace EnkaDotNet.Assets
{
    public abstract class BaseAssets : IAssets, IDisposable
    {
        private static readonly TimeSpan _initializationTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Upper bound on a single downloaded asset file. The largest real asset
        /// is well under this, so hitting it means the upstream source is misbehaving
        /// </summary>
        private const int MaxAssetBytes = 64 * 1024 * 1024;

        private readonly HttpClient _httpClient;
        private readonly string _fallbackDirectory;
        private readonly ConcurrentDictionary<string, object> _assetCache = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _assetFetchLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _loadingSemaphore;

        /// <summary>
        /// Cancellation for the in-flight asset initialization. Only ever written while
        /// <see cref="_initializationSemaphore"/> is held, and read by the fetch path that
        /// initialization drives, so no additional synchronization is required
        /// </summary>
        private CancellationToken _loadCancellation;

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

            int maxConcurrency = MathHelper.Clamp(Environment.ProcessorCount, 1, 8);
            _loadingSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        }

        public Task EnsureInitializedAsync() => EnsureInitializedAsync(CancellationToken.None);

        public async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (_isInitialized) return;
            if (_disposed) throw new ObjectDisposedException(GetType().Name);

            await _initializationSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_isInitialized) return;

                using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeoutSource.CancelAfter(_initializationTimeout);
                    _loadCancellation = timeoutSource.Token;
                    try
                    {
                        await Task.WhenAll(
                            LoadTextMapInternalAsync(Language),
                            LoadAssetsInternalAsync()).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException(
                            $"Asset initialization for {GameIdentifier} timed out after {_initializationTimeout.TotalMinutes} minutes.", ex);
                    }
                    finally
                    {
                        _loadCancellation = CancellationToken.None;
                    }
                }

                // Loaders have copied everything they need into their own maps, so the raw
                // deserialized payloads (notably the all-languages text map) can be released.
                _assetCache.Clear();
                _isInitialized = true;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        protected abstract Task LoadAssetsInternalAsync();
        protected abstract IReadOnlyDictionary<string, string> GetAssetFileUrls();

        /// <summary>
        /// Runs an asset loader under the shared concurrency limit
        /// </summary>
        protected async Task RunLoaderAsync(Func<Task> loadFunction)
        {
            await _loadingSemaphore.WaitAsync(_loadCancellation).ConfigureAwait(false);
            try
            {
                await loadFunction().ConfigureAwait(false);
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        protected virtual async Task LoadTextMapInternalAsync(string language)
        {
            try
            {
                var allLanguageMaps = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json").ConfigureAwait(false);

                if (allLanguageMaps.TryGetValue(language, out var languageSpecificMap))
                {
                    _textMap = new ConcurrentDictionary<string, string>(languageSpecificMap);
                    return;
                }

                _logger.LogWarning("Language code '{Language}' not found in the TextMap file for {GameIdentifier}. Available: {AvailableLanguages}", language, GameIdentifier, string.Join(", ", allLanguageMaps.Keys));

                string fallbackLanguage = "en";
                bool fallbackFound = allLanguageMaps.TryGetValue(fallbackLanguage, out var fallbackMap);
                if (!fallbackFound)
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
                        throw new InvalidOperationException($"No languages found in TextMap data for {GameIdentifier}");
                    }
                    fallbackLanguage = firstAvailableLanguage;
                    fallbackFound = allLanguageMaps.TryGetValue(fallbackLanguage, out fallbackMap);
                }

                if (fallbackFound)
                {
                    _logger.LogInformation("Falling back to '{FallbackLanguage}' language for {GameIdentifier}", fallbackLanguage, GameIdentifier);
                    _textMap = new ConcurrentDictionary<string, string>(fallbackMap);
                }
                else
                {
                    throw new InvalidOperationException($"Fallback language '{fallbackLanguage}' also not found for {GameIdentifier}");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TextMap for {GameIdentifier}", GameIdentifier);
                throw new InvalidOperationException($"Failed to load essential TextMap for {GameIdentifier}", ex);
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

        /// <summary>
        /// Resolves a text hash, returning <c>null</c> when the hash is unknown so callers can
        /// chain their own fallbacks. <see cref="GetText"/> echoes the hash back instead
        /// </summary>
        protected string TryGetText(string hash)
        {
            if (hash == null || _textMap == null) return null;
            return _textMap.TryGetValue(hash, out var text) && !string.IsNullOrEmpty(text) ? text : null;
        }

        protected async Task<string> FetchAssetAsync(string assetKey)
        {
            var assetFileUrls = GetAssetFileUrls();
            if (!assetFileUrls.TryGetValue(assetKey, out var url))
            {
                _logger.LogError("No URL defined for asset '{AssetKey}' in game {GameIdentifier}", assetKey, GameIdentifier);
                throw new InvalidOperationException($"No URL defined for asset '{assetKey}' in game {GameIdentifier}");
            }

            CancellationToken cancellationToken = _loadCancellation;

            try
            {
                string json;
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                    using (HttpResponseMessage response = await _httpClient
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        json = await ReadBoundedStringAsync(response, assetKey, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (_fallbackDirectory != null)
                {
                    await SaveAssetToDiskAsync(assetKey, json).ConfigureAwait(false);
                }

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
                        return await File.ReadAllTextAsync(localPath, cancellationToken).ConfigureAwait(false);
#else
                        using (var reader = new StreamReader(localPath))
                        {
                            return await reader.ReadToEndAsync().ConfigureAwait(false);
                        }
#endif
                    }
                }

                throw;
            }
        }

        private async Task<string> ReadBoundedStringAsync(HttpResponseMessage response, string assetKey, CancellationToken cancellationToken)
        {
            long? declaredLength = response.Content.Headers.ContentLength;
            if (declaredLength > MaxAssetBytes)
            {
                throw new InvalidOperationException(
                    $"Asset '{assetKey}' for {GameIdentifier} reports {declaredLength} bytes, above the {MaxAssetBytes} byte limit.");
            }

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                var builder = new StringBuilder(declaredLength.HasValue ? (int)Math.Min(declaredLength.Value, 1 << 20) : 1 << 16);
                var buffer = new byte[81920];
                var decoder = new UTF8Encoding(false).GetDecoder();
                var chars = new char[Encoding.UTF8.GetMaxCharCount(buffer.Length)];
                long total = 0;
                int read;

                while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    total += read;
                    if (total > MaxAssetBytes)
                    {
                        throw new InvalidOperationException(
                            $"Asset '{assetKey}' for {GameIdentifier} exceeded the {MaxAssetBytes} byte limit while downloading.");
                    }

                    int charCount = decoder.GetChars(buffer, 0, read, chars, 0);
                    builder.Append(chars, 0, charCount);
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Resolves the on-disk fallback path for an asset, rejecting keys that would escape
        /// the configured fallback directory
        /// </summary>
        private string GetFallbackPath(string assetKey)
        {
            string root = Path.GetFullPath(Path.Combine(_fallbackDirectory, GameIdentifier));
            string resolved = Path.GetFullPath(Path.Combine(root, assetKey));

            string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? root
                : root + Path.DirectorySeparatorChar;

            if (!resolved.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Asset key '{assetKey}' resolves outside the configured fallback directory.");
            }

            return resolved;
        }

        private async Task SaveAssetToDiskAsync(string assetKey, string json)
        {
            string path = null;
            string tempPath = null;
            try
            {
                path = GetFallbackPath(assetKey);
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // Write to a sibling temp file first so an interrupted write can never leave a
                // truncated file that a later run would happily load as the fallback
                tempPath = path + ".tmp";
#if NET8_0_OR_GREATER
                await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);
                File.Move(tempPath, path, overwrite: true);
#else
                using (var writer = new StreamWriter(tempPath, append: false))
                {
                    await writer.WriteAsync(json).ConfigureAwait(false);
                }
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);
#endif
                _logger.LogTrace("Asset '{AssetKey}' for {GameIdentifier} saved to fallback: {Path}", assetKey, GameIdentifier, path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save asset '{AssetKey}' for {GameIdentifier} to fallback directory.", assetKey, GameIdentifier);
                if (tempPath != null)
                {
                    try { File.Delete(tempPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
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

            await fetchLock.WaitAsync(_loadCancellation).ConfigureAwait(false);
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
                _loadingSemaphore.Dispose();
                foreach (var semaphore in _assetFetchLocks.Values)
                {
                    semaphore.Dispose();
                }
                _assetFetchLocks.Clear();
                _assetCache.Clear();
            }

            _disposed = true;
        }
    }
}
