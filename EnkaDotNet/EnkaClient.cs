using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Utils.Common;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using EnkaDotNet.Utils;
using EnkaDotNet.Assets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EnkaDotNet.Internal;

namespace EnkaDotNet
{
    public partial class EnkaClient : IEnkaClient
    {
        private readonly IHttpHelper _httpHelper;
        private readonly EnkaClientOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        private object _gameServiceHandler;

        private readonly ILogger<EnkaClient> _clientLogger;
        private Task _initializationTask;


        public GameType GameType => _options.GameType;
        public EnkaClientOptions Options => _options.Clone();

        public EnkaClient(
            IOptions<EnkaClientOptions> options,
            IHttpHelper httpHelper,
            IServiceProvider serviceProvider)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _clientLogger = _serviceProvider.GetService<ILogger<EnkaClient>>() ?? NullLogger<EnkaClient>.Instance;

            _initializationTask = InitializeGameSpecificServicesAsync();
        }

        private EnkaClient(EnkaClientOptions options, IHttpHelper httpHelper, IAssets assets, ILogger<EnkaClient> logger)
        {
            _options = options;
            _httpHelper = httpHelper;
            _clientLogger = logger;

            InitializeServiceHandlerWithAssets(assets);
            _initializationTask = Task.CompletedTask;
        }

        private void InitializeServiceHandlerWithAssets(IAssets assets)
        {
            switch (_options.GameType)
            {
                case GameType.Genshin:
                    _gameServiceHandler = new GenshinServiceHandler((IGenshinAssets)assets, _options, _httpHelper, _clientLogger);
                    break;
                case GameType.HSR:
                    _gameServiceHandler = new HSRServiceHandler((IHSRAssets)assets, _options, _httpHelper, _clientLogger);
                    break;
                case GameType.ZZZ:
                    _gameServiceHandler = new ZZZServiceHandler((IZZZAssets)assets, _options, _httpHelper, _clientLogger);
                    break;
                default:
                    throw new UnsupportedGameTypeException(_options.GameType, "Unsupported game type during direct service handler initialization.");
            }
        }


        public static async Task<IEnkaClient> CreateAsync(EnkaClientOptions options = null, ILogger<EnkaClient> logger = null)
        {
            options = options ?? new EnkaClientOptions();
            logger = logger ?? NullLogger<EnkaClient>.Instance;

            if (!Constants.IsGameTypeSupported(options.GameType))
            {
                throw new UnsupportedGameTypeException(options.GameType, $"Game type {options.GameType} is not currently supported for direct instantiation.");
            }

            var httpHelperClient = new HttpClient();
            string baseUrl = options.BaseUrl ?? Constants.GetApiBaseUrl(options.GameType);
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUriNet))
            {
                httpHelperClient.BaseAddress = baseUriNet;
            }
            else
            {
                httpHelperClient.BaseAddress = new Uri(new Uri(Constants.GetApiBaseUrl(options.GameType)), baseUrl);
            }
            httpHelperClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent ?? Constants.DefaultUserAgent);
            httpHelperClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            var httpHelperLogger = (ILogger<HttpHelper>)(logger is ILogger<HttpHelper> lh ? lh : NullLogger<HttpHelper>.Instance);
            var httpHelper = new HttpHelper(
                httpHelperClient,
                new MemoryCache(new MemoryCacheOptions()),
                Microsoft.Extensions.Options.Options.Create(options),
                httpHelperLogger
            );

            var assetHttpClient = new HttpClient();
            assetHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.DefaultUserAgent);

            IAssets assets;
            switch (options.GameType)
            {
                case GameType.Genshin:
                    var genshinLogger = (ILogger<GenshinAssets>)(logger is ILogger<GenshinAssets> lg ? lg : NullLogger<GenshinAssets>.Instance);
                    assets = await AssetsFactory.CreateGenshinAsync(options.Language, assetHttpClient, genshinLogger).ConfigureAwait(false);
                    break;
                case GameType.HSR:
                    var hsrLogger = (ILogger<HSRAssets>)(logger is ILogger<HSRAssets> lhsr ? lhsr : NullLogger<HSRAssets>.Instance);
                    assets = await AssetsFactory.CreateHSRAsync(options.Language, assetHttpClient, hsrLogger).ConfigureAwait(false);
                    break;
                case GameType.ZZZ:
                    var zzzLogger = (ILogger<ZZZAssets>)(logger is ILogger<ZZZAssets> lzzz ? lzzz : NullLogger<ZZZAssets>.Instance);
                    assets = await AssetsFactory.CreateZZZAsync(options.Language, assetHttpClient, zzzLogger).ConfigureAwait(false);
                    break;
                default:
                    throw new UnsupportedGameTypeException(options.GameType);
            }

            return new EnkaClient(options, httpHelper, assets, logger);
        }

        private async Task InitializeGameSpecificServicesAsync()
        {
            try
            {
                IAssets resolvedAssets;
                switch (_options.GameType)
                {
                    case GameType.Genshin:
                        var genshinAssetsTask = _serviceProvider.GetRequiredService<Task<IGenshinAssets>>();
                        resolvedAssets = await genshinAssetsTask.ConfigureAwait(false);
                        _gameServiceHandler = new GenshinServiceHandler((IGenshinAssets)resolvedAssets, _options, _httpHelper, _clientLogger);
                        break;
                    case GameType.HSR:
                        var hsrAssetsTask = _serviceProvider.GetRequiredService<Task<IHSRAssets>>();
                        resolvedAssets = await hsrAssetsTask.ConfigureAwait(false);
                        _gameServiceHandler = new HSRServiceHandler((IHSRAssets)resolvedAssets, _options, _httpHelper, _clientLogger);
                        break;
                    case GameType.ZZZ:
                        var zzzAssetsTask = _serviceProvider.GetRequiredService<Task<IZZZAssets>>();
                        resolvedAssets = await zzzAssetsTask.ConfigureAwait(false);
                        _gameServiceHandler = new ZZZServiceHandler((IZZZAssets)resolvedAssets, _options, _httpHelper, _clientLogger);
                        break;
                    default:
                        throw new UnsupportedGameTypeException(_options.GameType, $"Cannot initialize services for unsupported game type: {_options.GameType}");
                }
            }
            catch (InvalidOperationException ex)
            {
                _clientLogger.LogError(ex, "Failed to resolve game-specific services for GameType '{GameType}'. Ensure services were correctly registered.", _options.GameType);
                throw new EnkaClientConfigurationException($"Failed to resolve game-specific services for GameType '{_options.GameType}'. Ensure services were correctly registered with AddEnkaNetClient().", ex);
            }
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initializationTask != null && !_initializationTask.IsCompleted)
            {
                await _initializationTask.ConfigureAwait(false);
            }
        }

        private async Task EnsureGameTypeAndHandlerAsync(GameType expectedGameType, string operationDescription)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            if (_options.GameType != expectedGameType)
            {
                throw new NotSupportedException(
                    $"This operation ({operationDescription}) is only available for {expectedGameType}. " +
                    $"Current game type: {_options.GameType}");
            }
            if (_gameServiceHandler == null)
            {
                throw new InvalidOperationException($"Game service handler for {expectedGameType} not initialized for operation: {operationDescription}.");
            }
        }

        public async Task<ApiResponse> GetRawUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAndHandlerAsync(GameType.Genshin, nameof(GetRawUserResponseAsync)).ConfigureAwait(false);
            return await ((GenshinServiceHandler)_gameServiceHandler).GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<PlayerInfo> GetPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.Genshin, nameof(GetPlayerInfoAsync)).ConfigureAwait(false);
            return await ((GenshinServiceHandler)_gameServiceHandler).GetPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Character>> GetCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.Genshin, nameof(GetCharactersAsync)).ConfigureAwait(false);
            return await ((GenshinServiceHandler)_gameServiceHandler).GetCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetUserProfileAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.Genshin, nameof(GetUserProfileAsync)).ConfigureAwait(false);
            return await ((GenshinServiceHandler)_gameServiceHandler).GetUserProfileAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HSRApiResponse> GetRawHSRUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAndHandlerAsync(GameType.HSR, nameof(GetRawHSRUserResponseAsync)).ConfigureAwait(false);
            return await ((HSRServiceHandler)_gameServiceHandler).GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.HSR, nameof(GetHSRPlayerInfoAsync)).ConfigureAwait(false);
            return await ((HSRServiceHandler)_gameServiceHandler).GetHSRPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.HSR, nameof(GetHSRCharactersAsync)).ConfigureAwait(false);
            return await ((HSRServiceHandler)_gameServiceHandler).GetHSRCharactersAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ZZZApiResponse> GetRawZZZUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAndHandlerAsync(GameType.ZZZ, nameof(GetRawZZZUserResponseAsync)).ConfigureAwait(false);
            return await ((ZZZServiceHandler)_gameServiceHandler).GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.ZZZ, nameof(GetZZZPlayerInfoAsync)).ConfigureAwait(false);
            return await ((ZZZServiceHandler)_gameServiceHandler).GetZZZPlayerInfoAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAndHandlerAsync(GameType.ZZZ, nameof(GetZZZAgentsAsync)).ConfigureAwait(false);
            return await ((ZZZServiceHandler)_gameServiceHandler).GetZZZAgentsAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
        }

        public (long CurrentEntryCount, int ExpiredCountNotAvailable) GetCacheStats()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            return _httpHelper.GetCacheStats();
        }

        public void ClearCache()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            _httpHelper.ClearCache();
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

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
