using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Components.EnkaProfile;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Internal;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.Enka;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

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
        private bool _disposed = false;

        private readonly EnkaProfileServiceHandler _enkaProfileServiceHandler;
        private readonly EnkaDataMapper _enkaDataMapper;

        private readonly ConcurrentDictionary<string, IGenshinAssets> _genshinAssetsCache = new ConcurrentDictionary<string, IGenshinAssets>();
        private readonly ConcurrentDictionary<string, IHSRAssets> _hsrAssetsCache = new ConcurrentDictionary<string, IHSRAssets>();
        private readonly ConcurrentDictionary<string, IZZZAssets> _zzzAssetsCache = new ConcurrentDictionary<string, IZZZAssets>();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EnkaClient> _clientLogger;

        private const string DEFAULT_LANGUAGE = "en";

        /// <inheritdoc/>
        public EnkaClientOptions Options => _options.Clone();

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
            _clientLogger = _serviceProvider.GetService<ILogger<EnkaClient>>() ?? NullLogger<EnkaClient>.Instance;

            _enkaDataMapper = new EnkaDataMapper(_options);

            HttpClient enkaProfileHttpClient = _httpClientFactory.CreateClient("EnkaProfileClient");
            if (enkaProfileHttpClient.BaseAddress == null) enkaProfileHttpClient.BaseAddress = new Uri(Constants.DEFAULT_ENKA_PROFILE_API_BASE_URL);

            _enkaProfileServiceHandler = new EnkaProfileServiceHandler(
                _options,
                _httpHelper,
                _enkaDataMapper,
                _serviceProvider.GetService<ILogger<EnkaProfileServiceHandler>>() ?? NullLogger<EnkaProfileServiceHandler>.Instance,
                enkaProfileHttpClient
            );
        }

        /// <summary>
        /// Creates a new instance of the EnkaClient Recommended for scenarios without dependency injection
        /// Game specific assets will be loaded on demand based on the language specified in properties calls
        /// </summary>
        /// <param name="options">The options to configure the client</param>
        /// <param name="logger">The logger to use for logging</param>
        /// <param name="httpClientFactory">Optional HTTP client factory If null, a default one will be used</param>
        /// <returns>A task that represents the asynchronous operation The task result contains the created EnkaClient instance</returns>
        public static Task<IEnkaClient> CreateAsync(EnkaClientOptions options = null, ILogger<EnkaClient> logger = null, IHttpClientFactory httpClientFactory = null)
        {
            options = options ?? new EnkaClientOptions();
            logger = logger ?? NullLogger<EnkaClient>.Instance;

            httpClientFactory = httpClientFactory ?? new DefaultHttpClientFactory();

            var httpHelperHttpClient = httpClientFactory.CreateClient("DefaultEnkaClient");
            if (httpHelperHttpClient.BaseAddress == null) httpHelperHttpClient.BaseAddress = new Uri(options.BaseUrl);
            httpHelperHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? Constants.DefaultUserAgent);
            httpHelperHttpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            var httpHelper = new HttpHelper(
                httpHelperHttpClient,
                new MemoryCache(new MemoryCacheOptions()),
                Microsoft.Extensions.Options.Options.Create(options),
                (logger as ILogger<HttpHelper>) ?? NullLogger<HttpHelper>.Instance
            );

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(httpClientFactory);
            var dummyServiceProvider = serviceCollection.BuildServiceProvider();

            return Task.FromResult<IEnkaClient>(new EnkaClient(Microsoft.Extensions.Options.Options.Create(options), httpHelper, dummyServiceProvider, httpClientFactory));
        }

        private async Task<IGenshinAssets> GetGenshinAssetsAsync(string language)
        {
            language = language ?? DEFAULT_LANGUAGE;
            if (!_genshinAssetsCache.TryGetValue(language, out var assets))
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IGenshinAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                else
                {
                    var logger = _serviceProvider?.GetService<ILogger<GenshinAssets>>() ?? NullLogger<GenshinAssets>.Instance;
                    var httpClient = _httpClientFactory.CreateClient("GenshinAssetClient");
                    assets = await AssetsFactory.CreateGenshinAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                }
                _genshinAssetsCache.TryAdd(language, assets);
            }
            return assets;
        }

        private async Task<GenshinServiceHandler> GetGenshinHandlerAsync(string language)
        {
            var langAssets = await GetGenshinAssetsAsync(language ?? DEFAULT_LANGUAGE).ConfigureAwait(false);
            return new GenshinServiceHandler(langAssets, _options, _httpHelper, _clientLogger);
        }

        private async Task<IHSRAssets> GetHSRAssetsAsync(string language)
        {
            language = language ?? DEFAULT_LANGUAGE;
            if (!_hsrAssetsCache.TryGetValue(language, out var assets))
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IHSRAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                else
                {
                    var logger = _serviceProvider?.GetService<ILogger<HSRAssets>>() ?? NullLogger<HSRAssets>.Instance;
                    var httpClient = _httpClientFactory.CreateClient("HSRAssetClient");
                    assets = await AssetsFactory.CreateHSRAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                }
                _hsrAssetsCache.TryAdd(language, assets);
            }
            return assets;
        }

        private async Task<HSRServiceHandler> GetHSRHandlerAsync(string language)
        {
            var langAssets = await GetHSRAssetsAsync(language ?? DEFAULT_LANGUAGE).ConfigureAwait(false);
            return new HSRServiceHandler(langAssets, _options, _httpHelper, _clientLogger);
        }

        private async Task<IZZZAssets> GetZZZAssetsAsync(string language)
        {
            language = language ?? DEFAULT_LANGUAGE;
            if (!_zzzAssetsCache.TryGetValue(language, out var assets))
            {
                var assetsFactoryFunc = _serviceProvider?.GetService<Func<string, Task<IZZZAssets>>>();
                if (assetsFactoryFunc != null)
                {
                    assets = await assetsFactoryFunc(language).ConfigureAwait(false);
                }
                else
                {
                    var logger = _serviceProvider?.GetService<ILogger<ZZZAssets>>() ?? NullLogger<ZZZAssets>.Instance;
                    var httpClient = _httpClientFactory.CreateClient("ZZZAssetClient");
                    assets = await AssetsFactory.CreateZZZAssetsAsync(language, httpClient, logger).ConfigureAwait(false);
                }
                _zzzAssetsCache.TryAdd(language, assets);
            }
            return assets;
        }

        private async Task<ZZZServiceHandler> GetZZZHandlerAsync(string language)
        {
            var langAssets = await GetZZZAssetsAsync(language ?? DEFAULT_LANGUAGE).ConfigureAwait(false);
            return new ZZZServiceHandler(langAssets, _options, _httpHelper, _clientLogger);
        }

        /// <inheritdoc/>
        public async Task<EnkaProfileResponse> GetRawEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            return await _enkaProfileServiceHandler.GetRawEnkaProfileByUsernameAsync(username, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EnkaUserProfile> GetEnkaProfileByUsernameAsync(string username, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            return await _enkaProfileServiceHandler.GetEnkaProfileByUsernameAsync(username, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> GetGenshinRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<PlayerInfo> GetGenshinPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Character>> GetGenshinCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetGenshinUserProfileAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetGenshinHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetUserProfileAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HSRApiResponse> GetHSRRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetHSRPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetHSRHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetHSRCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ZZZApiResponse> GetZZZRawUserResponseAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetZZZPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, string language = null, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            var handler = await GetZZZHandlerAsync(language).ConfigureAwait(false);
            return await handler.GetZZZAgentsAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
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
            _clientLogger.LogInformation("Cleared EnkaClient language asset caches");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpHelper?.Dispose();
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
