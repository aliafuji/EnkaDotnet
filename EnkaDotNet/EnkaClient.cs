using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
using EnkaDotNet.Enums;
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

        private readonly ConcurrentDictionary<string, Lazy<Task<IGenshinAssets>>> _genshinAssetsCache = new ConcurrentDictionary<string, Lazy<Task<IGenshinAssets>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<IHSRAssets>>> _hsrAssetsCache = new ConcurrentDictionary<string, Lazy<Task<IHSRAssets>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<IZZZAssets>>> _zzzAssetsCache = new ConcurrentDictionary<string, Lazy<Task<IZZZAssets>>>();

        private readonly ConcurrentDictionary<string, Lazy<Task<GenshinServiceHandler>>> _genshinHandlerCache = new ConcurrentDictionary<string, Lazy<Task<GenshinServiceHandler>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<HSRServiceHandler>>> _hsrHandlerCache = new ConcurrentDictionary<string, Lazy<Task<HSRServiceHandler>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<ZZZServiceHandler>>> _zzzHandlerCache = new ConcurrentDictionary<string, Lazy<Task<ZZZServiceHandler>>>();

        private readonly IHttpClientFactory _httpClientFactory;

        private const string DEFAULT_LANGUAGE = "en";
        private static readonly int maxConcurrency = MathHelper.Clamp(Environment.ProcessorCount, 1, 8);


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

            IEnkaCache enkaCache;
            if (options.CacheProvider == CacheProvider.Custom && customCache != null)
            {
                enkaCache = customCache;
            }
            else if (options.CacheProvider == CacheProvider.Memory && cache != null)
            {
                enkaCache = CacheFactory.CreateCache(options, cache);
            }
            else
            {
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

            if (options.PreloadedLanguages != null && options.PreloadedLanguages.Count > 0)
            {
                await client.PreloadAssetsAsync(options.PreloadedLanguages).ConfigureAwait(false);
            }

            return client;
        }

        private Task<IGenshinAssets> GetGenshinAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _genshinAssetsCache.GetOrAdd(language, lang =>
                new Lazy<Task<IGenshinAssets>>(() => LoadGenshinAssetsAsync(lang))
            ).Value;
        }

        private async Task<IGenshinAssets> LoadGenshinAssetsAsync(string language)
        {
            try
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IGenshinAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    return await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                var logger = _loggerFactory.CreateLogger<GenshinAssets>();
                var httpClient = _httpClientFactory.CreateClient("GenshinAssetClient");
                return await AssetsFactory.CreateGenshinAssetsAsync(language, httpClient, logger, _options.AssetFallbackDirectory).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _genshinAssetsCache.TryRemove(language, out _);
                Logger.LogError(ex, "Timeout while fetching Genshin assets for language {Language}", language);
                throw new EnkaNetworkException($"Timeout while fetching Genshin assets for language {language}", ex);
            }
        }

        private Task<GenshinServiceHandler> GetGenshinHandlerAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _genshinHandlerCache.GetOrAdd(language, lang =>
                new Lazy<Task<GenshinServiceHandler>>(async () =>
                {
                    var langAssets = await GetGenshinAssetsAsync(lang).ConfigureAwait(false);
                    return new GenshinServiceHandler(langAssets, _options, _httpHelper, Logger);
                })
            ).Value;
        }

        private Task<IHSRAssets> GetHSRAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _hsrAssetsCache.GetOrAdd(language, lang =>
                new Lazy<Task<IHSRAssets>>(() => LoadHSRAssetsAsync(lang))
            ).Value;
        }

        private async Task<IHSRAssets> LoadHSRAssetsAsync(string language)
        {
            try
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IHSRAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    return await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                var logger = _loggerFactory.CreateLogger<HSRAssets>();
                var httpClient = _httpClientFactory.CreateClient("HSRAssetClient");
                return await AssetsFactory.CreateHSRAssetsAsync(language, httpClient, logger, _options.AssetFallbackDirectory).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _hsrAssetsCache.TryRemove(language, out _);
                Logger.LogError(ex, "Timeout while fetching HSR assets for language {Language}", language);
                throw new EnkaNetworkException($"Timeout while fetching HSR assets for language {language}", ex);
            }
        }

        private Task<HSRServiceHandler> GetHSRHandlerAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _hsrHandlerCache.GetOrAdd(language, lang =>
                new Lazy<Task<HSRServiceHandler>>(async () =>
                {
                    var langAssets = await GetHSRAssetsAsync(lang).ConfigureAwait(false);
                    return new HSRServiceHandler(langAssets, _options, _httpHelper, Logger);
                })
            ).Value;
        }

        private Task<IZZZAssets> GetZZZAssetsAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _zzzAssetsCache.GetOrAdd(language, lang =>
                new Lazy<Task<IZZZAssets>>(() => LoadZZZAssetsAsync(lang))
            ).Value;
        }

        private async Task<IZZZAssets> LoadZZZAssetsAsync(string language)
        {
            try
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IZZZAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    return await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                var logger = _loggerFactory.CreateLogger<ZZZAssets>();
                var httpClient = _httpClientFactory.CreateClient("ZZZAssetClient");
                return await AssetsFactory.CreateZZZAssetsAsync(language, httpClient, logger, _options.AssetFallbackDirectory).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                _zzzAssetsCache.TryRemove(language, out _);
                Logger.LogError(ex, "Timeout while fetching ZZZ assets for language {Language}", language);
                throw new EnkaNetworkException($"Timeout while fetching ZZZ assets for language {language}", ex);
            }
        }

        private Task<ZZZServiceHandler> GetZZZHandlerAsync(string language)
        {
            language = (language ?? DEFAULT_LANGUAGE).ToLowerInvariant();
            return _zzzHandlerCache.GetOrAdd(language, lang =>
                new Lazy<Task<ZZZServiceHandler>>(async () =>
                {
                    var langAssets = await GetZZZAssetsAsync(lang).ConfigureAwait(false);
                    return new ZZZServiceHandler(langAssets, _options, _httpHelper, Logger);
                })
            ).Value;
        }

        private async Task<T> ExecuteApiCallAsync<T>(Func<Task<T>> apiCall)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            return await apiCall().ConfigureAwait(false);
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
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                var assets = await GetGenshinAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.Genshin.DataMapper(assets, _options);
                
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
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                var assets = await GetHSRAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.HSR.HSRDataMapper(assets, _options);
                
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
                var rawBuilds = await _enkaProfileServiceHandler.GetRawBuildsByUsernameAsync(username, hoyoHash, bypassCache, cancellationToken).ConfigureAwait(false);
                
                var assets = await GetZZZAssetsAsync(language).ConfigureAwait(false);
                var dataMapper = new Utils.ZZZ.ZZZDataMapper(assets, _options);
                
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

        private static string ResolveLanguage(Language language)
            => language.GetEnumMemberValue() ?? language.ToString().ToLowerInvariant();

        /// <inheritdoc/>
        public Task<ApiResponse> GetGenshinRawUserResponseAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetGenshinRawUserResponseAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<PlayerInfo> GetGenshinPlayerInfoAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetGenshinPlayerInfoAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<IReadOnlyList<Character>> GetGenshinCharactersAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetGenshinCharactersAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetGenshinUserProfileAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetGenshinUserProfileAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<HSRApiResponse> GetHSRRawUserResponseAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetHSRRawUserResponseAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetHSRPlayerInfoAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetHSRCharactersAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<ZZZApiResponse> GetZZZRawUserResponseAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetZZZRawUserResponseAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetZZZPlayerInfoAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetZZZAgentsAsync(uid, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<Dictionary<string, List<GenshinBuild>>> GetGenshinBuildsByUsernameAsync(string username, string hoyoHash, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetGenshinBuildsByUsernameAsync(username, hoyoHash, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<Dictionary<string, List<HSRBuild>>> GetHSRBuildsByUsernameAsync(string username, string hoyoHash, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetHSRBuildsByUsernameAsync(username, hoyoHash, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public Task<Dictionary<string, List<ZZZBuild>>> GetZZZBuildsByUsernameAsync(string username, string hoyoHash, Language language, bool bypassCache = false, CancellationToken cancellationToken = default) =>
            GetZZZBuildsByUsernameAsync(username, hoyoHash, ResolveLanguage(language), bypassCache, cancellationToken);

        /// <inheritdoc/>
        public async Task PreloadAssetsAsync(IEnumerable<Language> languages)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var langList = new List<string>();
            foreach (var lang in languages)
            {
                var normalized = lang.GetEnumMemberValue() ?? lang.ToString().ToLowerInvariant();
                if (seen.Add(normalized))
                    langList.Add(normalized);
            }

            if (langList.Count == 0) return;

            Logger.LogInformation(EnkaEventIds.AssetPreloadStart,
                "Preloading assets for {Count} language(s): {Languages}",
                langList.Count, string.Join(", ", langList));

            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
            using var activity = EnkaTelemetry.ActivitySource.StartActivity("EnkaClient.PreloadAssets");
            activity?.SetTag("enka.languages", string.Join(",", langList));

            var tasks = new List<Task>(langList.Count * 3);
            foreach (var lang in langList)
            {
                tasks.Add(RunWithSemaphoreAsync(semaphore, () => GetGenshinAssetsAsync(lang)));
                tasks.Add(RunWithSemaphoreAsync(semaphore, () => GetHSRAssetsAsync(lang)));
                tasks.Add(RunWithSemaphoreAsync(semaphore, () => GetZZZAssetsAsync(lang)));
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
                Logger.LogInformation(EnkaEventIds.AssetPreloadDone, "Finished preloading all specified assets.");
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        private static async Task RunWithSemaphoreAsync(SemaphoreSlim semaphore, Func<Task> load)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await load().ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
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
            _genshinHandlerCache.Clear();
            _hsrHandlerCache.Clear();
            _zzzHandlerCache.Clear();
            Logger.LogInformation("Cleared EnkaClient language asset and handler caches");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    (_httpHelper as IDisposable)?.Dispose();
                    (_httpClientFactory as IDisposable)?.Dispose();
                    _genshinAssetsCache.Clear();
                    _hsrAssetsCache.Clear();
                    _zzzAssetsCache.Clear();
                    _genshinHandlerCache.Clear();
                    _hsrHandlerCache.Clear();
                    _zzzHandlerCache.Clear();
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
        /// A fallback <see cref="IHttpClientFactory"/> used when the caller does not provide one.
        /// <para>
        /// On .NET 5+, uses <c>SocketsHttpHandler</c> with <c>PooledConnectionLifetime</c> to get
        /// proper connection pooling and periodic DNS refresh matching the behaviour of the
        /// ASP.NET Core <c>IHttpClientFactory</c> for singleton instances.
        /// </para>
        /// <para>
        /// On netstandard2.0, <c>SocketsHttpHandler</c> is unavailable a plain singleton
        /// <c>HttpClient</c> is used instead. DNS changes will not be picked up until the process
        /// restarts. For production netstandard2.0 scenarios supply your own
        /// <c>IHttpClientFactory</c> via the <c>httpClientFactory</c> parameter of
        /// <see cref="CreateAsync"/>.
        /// </para>
        /// </summary>
        private sealed class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
        {
            private readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>();
            private bool _disposed;

            public HttpClient CreateClient(string name)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(DefaultHttpClientFactory));
                return _clients.GetOrAdd(name, CreateHttpClient);
            }

            private static HttpClient CreateHttpClient(string name)
            {
#if NET5_0_OR_GREATER
                var handler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
                    MaxConnectionsPerServer = 10,
                    EnableMultipleHttp2Connections = true,
                };
                var client = new HttpClient(handler);
#else
                var client = new HttpClient();
#endif
                if (name == "EnkaProfileClient" || name == "DefaultEnkaClient")
                    client.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);
                return client;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                foreach (var client in _clients.Values)
                    client.Dispose();
                _clients.Clear();
                GC.SuppressFinalize(this);
            }
        }
    }
}