using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Caching;
using EnkaDotNet.Components.EnkaProfile;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Internal;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.Enka;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnkaDotNet
{
    /// <summary>
    /// Client for interacting with the Enka.Network API
    /// </summary>
    public partial class EnkaClient : IEnkaClient
    {
        private readonly IHttpHelper _httpHelper;
        private readonly EnkaClientOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private bool _disposed = false;

        private readonly EnkaProfileServiceHandler _enkaProfileServiceHandler;
        private readonly EnkaDataMapper _enkaDataMapper;

        private readonly ConcurrentDictionary<string, IGenshinAssets> _genshinAssetsCache = new ConcurrentDictionary<string, IGenshinAssets>();
        private readonly ConcurrentDictionary<string, IHSRAssets> _hsrAssetsCache = new ConcurrentDictionary<string, IHSRAssets>();
        private readonly ConcurrentDictionary<string, IZZZAssets> _zzzAssetsCache = new ConcurrentDictionary<string, IZZZAssets>();

        private readonly IHttpClientFactory _httpClientFactory;

        private const string DEFAULT_LANGUAGE = "en";
        private static readonly int maxConcurrency = Meth.Clamp(Environment.ProcessorCount, 1, 8);
        private static readonly SemaphoreSlim _preloadSemaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        /// <inheritdoc/>
        public EnkaClientOptions Options => _options.Clone();

        /// <inheritdoc/>
        public ILogger<EnkaClient> Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnkaClient"/> class with the specified options, HTTP helper, service provider, and HTTP client factory
        /// </summary>
        /// <param name="options">The options to configure the client</param>
        /// <param name="httpHelper">The HTTP helper for making requests</param>
        /// <param name="serviceProvider">The service provider for resolving dependencies</param>
        /// <param name="httpClientFactory">The HTTP client factory for creating named HTTP clients</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null</exception>
        public EnkaClient(
            IOptions<EnkaClientOptions> options,
            IHttpHelper httpHelper,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            _loggerFactory = _serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            Logger = _loggerFactory.CreateLogger<EnkaClient>();

            _enkaDataMapper = new EnkaDataMapper(_options);

            HttpClient enkaProfileHttpClient = _httpClientFactory.CreateClient("EnkaProfileClient");
            if (enkaProfileHttpClient.BaseAddress == null) enkaProfileHttpClient.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);

            _enkaProfileServiceHandler = new EnkaProfileServiceHandler(
                _options,
                _httpHelper,
                _enkaDataMapper,
                _loggerFactory.CreateLogger<EnkaProfileServiceHandler>(),
                enkaProfileHttpClient
            );
        }

        /// <summary>
        /// Creates a new instance of the EnkaClient. Recommended for scenarios without dependency injection.
        /// Game specific assets will be loaded on demand based on the language specified in API calls unless preloading is configured in options.
        /// </summary>
        /// <param name="options">The options to configure the client. If provided, `PreloadLanguages` will trigger automatic asset loading.</param>
        /// <param name="loggerFactory">The logger factory to use for creating loggers.</param>
        /// <param name="cache">Optional custom memory cache instance (for backward compatibility).</param>
        /// <param name="httpClientFactory">Optional HTTP client factory. If null, a default one will be used.</param>
        /// <param name="customCache">Optional custom IEnkaCache instance for Custom cache provider.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created EnkaClient instance.</returns>
        public static async Task<IEnkaClient> CreateAsync(
            EnkaClientOptions options = null, 
            ILoggerFactory loggerFactory = null, 
            IMemoryCache cache = null, 
            IHttpClientFactory httpClientFactory = null,
            IEnkaCache customCache = null)
        {
            options = options ?? new EnkaClientOptions();
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();

            var httpHelperHttpClient = httpClientFactory.CreateClient("DefaultEnkaClient");
            if (httpHelperHttpClient.BaseAddress == null) httpHelperHttpClient.BaseAddress = new Uri(options.BaseUrl);
            httpHelperHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? Constants.DefaultUserAgent);
            httpHelperHttpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            // Create the appropriate cache provider based on options
            IEnkaCache enkaCache;
            if (options.CacheProvider == CacheProvider.Custom && customCache != null)
            {
                enkaCache = customCache;
            }
            else if (options.CacheProvider == CacheProvider.Memory && cache != null)
            {
                // Use provided memory cache for backward compatibility
                enkaCache = CacheFactory.CreateCache(options, cache);
            }
            else
            {
                // Create cache based on provider setting
                enkaCache = CacheFactory.CreateCache(options, cache, customCache);
            }

            var httpHelper = new HttpHelper(
                httpHelperHttpClient,
                enkaCache,
                Microsoft.Extensions.Options.Options.Create(options),
                loggerFactory.CreateLogger<HttpHelper>()
            );

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(httpClientFactory);
            serviceCollection.AddSingleton(loggerFactory);
            var dummyServiceProvider = serviceCollection.BuildServiceProvider();

            var client = new EnkaClient(Microsoft.Extensions.Options.Options.Create(options), httpHelper, dummyServiceProvider, httpClientFactory);

            if (options.PreloadLanguages != null && options.PreloadLanguages.Count > 0)
            {
                await client.PreloadAssetsAsync(options.PreloadLanguages).ConfigureAwait(false);
            }

            return client;
        }

        private async Task<IGenshinAssets> GetGenshinAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            if (!_genshinAssetsCache.TryGetValue(language, out var assets))
            {
                try
                {
                    var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IGenshinAssets>>>();
                    if (assetsFactoryFunc != null)
                    {
                        assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                    }
                    else
                    {
                        var logger = _loggerFactory.CreateLogger<GenshinAssets>();
                        var httpClient = _httpClientFactory.CreateClient("GenshinAssetClient");
                        assets = await AssetsFactory.CreateGenshinAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                    }
                    _genshinAssetsCache.TryAdd(language, assets);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogError(ex, "Timeout while fetching Genshin assets for language {Language}", language);
                    throw new EnkaNetworkException($"Timeout while fetching Genshin assets for language {language}", ex);
                }
            }
            return assets;
        }

        private async Task<GenshinServiceHandler> GetGenshinHandlerAsync(string language)
        {
            var langAssets = await GetGenshinAssetsAsync(language).ConfigureAwait(false);
            return new GenshinServiceHandler(langAssets, _options, _httpHelper, Logger);
        }

        private async Task<IHSRAssets> GetHSRAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            if (!_hsrAssetsCache.TryGetValue(language, out var assets))
            {
                try
                {
                    var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IHSRAssets>>>();
                    if (assetsFactoryFunc != null)
                    {
                        assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                    }
                    else
                    {
                        var logger = _loggerFactory.CreateLogger<HSRAssets>();
                        var httpClient = _httpClientFactory.CreateClient("HSRAssetClient");
                        assets = await AssetsFactory.CreateHSRAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                    }
                    _hsrAssetsCache.TryAdd(language, assets);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogError(ex, "Timeout while fetching HSR assets for language {Language}", language);
                    throw new EnkaNetworkException($"Timeout while fetching HSR assets for language {language}", ex);
                }
            }
            return assets;
        }

        private async Task<HSRServiceHandler> GetHSRHandlerAsync(string language)
        {
            var langAssets = await GetHSRAssetsAsync(language).ConfigureAwait(false);
            return new HSRServiceHandler(langAssets, _options, _httpHelper, Logger);
        }

        private async Task<IZZZAssets> GetZZZAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            if (!_zzzAssetsCache.TryGetValue(language, out var assets))
            {
                try
                {
                    var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IZZZAssets>>>();
                    if (assetsFactoryFunc != null)
                    {
                        assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                    }
                    else
                    {
                        var logger = _loggerFactory.CreateLogger<ZZZAssets>();
                        var httpClient = _httpClientFactory.CreateClient("ZZZAssetClient");
                        assets = await AssetsFactory.CreateZZZAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                    }
                    _zzzAssetsCache.TryAdd(language, assets);
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogError(ex, "Timeout while fetching ZZZ assets for language {Language}", language);
                    throw new EnkaNetworkException($"Timeout while fetching ZZZ assets for language {language}", ex);
                }
            }
            return assets;
        }

        private async Task<ZZZServiceHandler> GetZZZHandlerAsync(string language)
        {
            var langAssets = await GetZZZAssetsAsync(language).ConfigureAwait(false);
            return new ZZZServiceHandler(langAssets, _options, _httpHelper, Logger);
        }

        private async Task<T> ExecuteApiCallAsync<T>(Func<Task<T>> apiCall)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            try
            {
                return await apiCall().ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogError(ex, "The API request timed out.");
                throw new EnkaNetworkException("The request timed out.", ex);
            }
        }

        /// <inheritdoc/>
        public Task<EnkaProfileResponse> GetRawEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(() => _enkaProfileServiceHandler.GetRawEnkaProfileByUsernameAsync(username, bypassCache, cancellationToken));

        /// <inheritdoc/>
        public Task<EnkaUserProfile> GetEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(() => _enkaProfileServiceHandler.GetEnkaProfileByUsernameAsync(username, bypassCache, cancellationToken));

        /// <inheritdoc/>
        public Task<ApiResponse> GetGenshinRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<PlayerInfo> GetGenshinPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<IReadOnlyList<Character>> GetGenshinCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetGenshinUserProfileAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetUserProfileAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<HSRApiResponse> GetHSRRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetHSRPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetHSRCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<ZZZApiResponse> GetZZZRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetZZZPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
                return await handler.GetZZZAgentsAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            });

        /// <inheritdoc/>
        public Task<Dictionary<string, List<GenshinBuild>>> GetGenshinBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                // Fetch raw builds from the API
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                // Get the Genshin handler with the appropriate language assets
                var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
                var assets = await GetGenshinAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.Genshin.DataMapper(assets, _options);
                
                // Map raw builds to GenshinBuild components
                var result = new Dictionary<string, List<GenshinBuild>>();
                
                foreach (var kvp in rawBuilds)
                {
                    var characterId = kvp.Key;
                    var buildList = new List<GenshinBuild>();
                    
                    foreach (var rawBuild in kvp.Value)
                    {
                        var build = new GenshinBuild
                        {
                            Id = rawBuild.Id,
                            Name = rawBuild.Name ?? string.Empty,
                            Order = rawBuild.Order,
                            IsLive = rawBuild.Live,
                            CharacterId = rawBuild.AvatarId
                        };
                        
                        // Parse the avatar_data to get the character
                        if (rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                            rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            try
                            {
#if NET8_0_OR_GREATER
                                var avatarInfo = System.Text.Json.JsonSerializer.Deserialize<Models.Genshin.AvatarInfoModel>(
                                    rawBuild.AvatarData.GetRawText(), 
                                    Serialization.EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                                var avatarInfo = System.Text.Json.JsonSerializer.Deserialize<Models.Genshin.AvatarInfoModel>(
                                    rawBuild.AvatarData.GetRawText());
#pragma warning restore IL2026, IL3050
#endif
                                if (avatarInfo != null)
                                {
                                    build.Character = dataMapper.MapCharacter(avatarInfo);
                                }
                            }
                            catch (System.Text.Json.JsonException ex)
                            {
                                Logger.LogWarning(ex, "Failed to deserialize avatar_data for build {BuildId} in character {CharacterId}", rawBuild.Id, characterId);
                            }
                        }
                        
                        buildList.Add(build);
                    }
                    
                    result[characterId] = buildList;
                }
                
                return result;
            });

        /// <inheritdoc/>
        public Task<Dictionary<string, List<HSRBuild>>> GetHSRBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                // Fetch raw builds from the API
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                // Get the HSR assets with the appropriate language
                var assets = await GetHSRAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.HSR.HSRDataMapper(assets, _options);
                
                // Map raw builds to HSRBuild components
                var result = new Dictionary<string, List<HSRBuild>>();
                
                foreach (var kvp in rawBuilds)
                {
                    var characterId = kvp.Key;
                    var buildList = new List<HSRBuild>();
                    
                    foreach (var rawBuild in kvp.Value)
                    {
                        var build = new HSRBuild
                        {
                            Id = rawBuild.Id,
                            Name = rawBuild.Name ?? string.Empty,
                            Order = rawBuild.Order,
                            IsLive = rawBuild.Live,
                            CharacterId = rawBuild.AvatarId
                        };
                        
                        // Parse the avatar_data to get the character
                        if (rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                            rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            try
                            {
#if NET8_0_OR_GREATER
                                var avatarDetail = System.Text.Json.JsonSerializer.Deserialize<Models.HSR.HSRAvatarDetail>(
                                    rawBuild.AvatarData.GetRawText(), 
                                    Serialization.EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                                var avatarDetail = System.Text.Json.JsonSerializer.Deserialize<Models.HSR.HSRAvatarDetail>(
                                    rawBuild.AvatarData.GetRawText());
#pragma warning restore IL2026, IL3050
#endif
                                if (avatarDetail != null)
                                {
                                    build.Character = dataMapper.MapCharacter(avatarDetail);
                                }
                            }
                            catch (System.Text.Json.JsonException ex)
                            {
                                Logger.LogWarning(ex, "Failed to deserialize avatar_data for HSR build {BuildId} in character {CharacterId}", rawBuild.Id, characterId);
                            }
                        }
                        
                        buildList.Add(build);
                    }
                    
                    result[characterId] = buildList;
                }
                
                return result;
            });

        /// <inheritdoc/>
        public Task<Dictionary<string, List<ZZZBuild>>> GetZZZBuildsByUsernameAsync(string username, string hoyoHash, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            ExecuteApiCallAsync(async () =>
            {
                // Fetch raw builds from the API
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                // Get the ZZZ assets with the appropriate language
                var assets = await GetZZZAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.ZZZ.ZZZDataMapper(assets, _options);
                
                // Map raw builds to ZZZBuild components
                var result = new Dictionary<string, List<ZZZBuild>>();
                
                foreach (var kvp in rawBuilds)
                {
                    var agentId = kvp.Key;
                    var buildList = new List<ZZZBuild>();
                    
                    foreach (var rawBuild in kvp.Value)
                    {
                        var build = new ZZZBuild
                        {
                            Id = rawBuild.Id,
                            Name = rawBuild.Name ?? string.Empty,
                            Order = rawBuild.Order,
                            IsLive = rawBuild.Live,
                            AgentId = rawBuild.AvatarId
                        };
                        
                        // Parse the avatar_data to get the agent
                        if (rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Undefined &&
                            rawBuild.AvatarData.ValueKind != System.Text.Json.JsonValueKind.Null)
                        {
                            try
                            {
#if NET8_0_OR_GREATER
                                var avatarModel = System.Text.Json.JsonSerializer.Deserialize<Models.ZZZ.ZZZAvatarModel>(
                                    rawBuild.AvatarData.GetRawText(), 
                                    Serialization.EnkaJsonContext.Default.Options);
#else
#pragma warning disable IL2026, IL3050
                                var avatarModel = System.Text.Json.JsonSerializer.Deserialize<Models.ZZZ.ZZZAvatarModel>(
                                    rawBuild.AvatarData.GetRawText());
#pragma warning restore IL2026, IL3050
#endif
                                if (avatarModel != null)
                                {
                                    build.Agent = dataMapper.MapAgent(avatarModel);
                                }
                            }
                            catch (System.Text.Json.JsonException ex)
                            {
                                Logger.LogWarning(ex, "Failed to deserialize avatar_data for ZZZ build {BuildId} in agent {AgentId}", rawBuild.Id, agentId);
                            }
                        }
                        
                        buildList.Add(build);
                    }
                    
                    result[agentId] = buildList;
                }
                
                return result;
            });

        /// <inheritdoc/>
        public async Task PreloadAssetsAsync(IEnumerable<string> languages)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));

            var tasks = new List<Task>();
            foreach (var lang in languages)
            {
                if (string.IsNullOrWhiteSpace(lang)) continue;

                await _preloadSemaphore.WaitAsync().ConfigureAwait(false);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        Logger.LogInformation("Preloading assets for language: {Language}", lang);
                        await GetGenshinAssetsAsync(lang).ConfigureAwait(false);
                        await GetHSRAssetsAsync(lang).ConfigureAwait(false);
                        await GetZZZAssetsAsync(lang).ConfigureAwait(false);
                    }
                    finally
                    {
                        _preloadSemaphore.Release();
                    }
                }));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
                Logger.LogInformation("Finished preloading all specified assets.");
            }
            catch (EnkaNetworkException ex)
            {
                Logger.LogError(ex, "Failed to preload assets due to a network timeout.");
                throw;
            }
        }

        /// <inheritdoc/>
        public (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            return _httpHelper.GetCacheStats();
        }

        /// <inheritdoc/>
        public void ClearCache()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            _httpHelper.ClearCache();
            _genshinAssetsCache.Clear();
            _hsrAssetsCache.Clear();
            _zzzAssetsCache.Clear();
            Logger.LogInformation("Cleared EnkaClient language asset caches");
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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// A basic implementation of IHttpClientFactory for use when one is not provided
        /// Warning: This implementation is not recommended for production use as it lacks advanced features like configurable handlers and retry policies
        /// </summary>
        private class DefaultHttpClientFactory : IHttpClientFactory
        {
            public HttpClient CreateClient(string name)
            {
                var client = new HttpClient();
                if (name == "EnkaProfileClient")
                {
                    client.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);
                }
                else if (name == "GenshinAssetClient" || name == "HSRAssetClient" || name == "ZZZAssetClient")
                {
                }
                else if (name == "DefaultEnkaClient")
                {
                    client.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);
                }
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                return client;
            }
        }
    }
}