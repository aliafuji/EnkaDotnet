using System;
using System.Collections.Generic;
using System.Linq;
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
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Utils.HSR;
using EnkaDotNet.Utils.ZZZ;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using EnkaDotNet.Utils;
using EnkaDotNet.Assets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnkaDotNet
{
    public partial class EnkaClient : IEnkaClient
    {
        private readonly IHttpHelper _httpHelper;
        private readonly EnkaClientOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        private IGenshinAssets _genshinAssets;
        private DataMapper _genshinDataMapper;
        private IHSRAssets _hsrAssets;
        private HSRDataMapper _hsrDataMapper;
        private IZZZAssets _zzzAssets;
        private ZZZDataMapper _zzzDataMapper;

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

            AssignAssetsAndMappers(assets);
            _initializationTask = Task.CompletedTask;
        }

        private void AssignAssetsAndMappers(IAssets assets)
        {
            switch (_options.GameType)
            {
                case GameType.Genshin:
                    _genshinAssets = (IGenshinAssets)assets;
                    _genshinDataMapper = new DataMapper(_genshinAssets, _options);
                    break;
                case GameType.HSR:
                    _hsrAssets = (IHSRAssets)assets;
                    _hsrDataMapper = new HSRDataMapper(_hsrAssets, _options);
                    break;
                case GameType.ZZZ:
                    _zzzAssets = (IZZZAssets)assets;
                    _zzzDataMapper = new ZZZDataMapper(_zzzAssets, _options);
                    break;
                default:
                    throw new UnsupportedGameTypeException(_options.GameType, "Unsupported game type during asset assignment.");
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
                switch (_options.GameType)
                {
                    case GameType.Genshin:
                        var genshinAssetsTask = _serviceProvider.GetRequiredService<Task<IGenshinAssets>>();
                        _genshinAssets = await genshinAssetsTask.ConfigureAwait(false);
                        _genshinDataMapper = _serviceProvider.GetRequiredService<DataMapper>();
                        break;
                    case GameType.HSR:
                        var hsrAssetsTask = _serviceProvider.GetRequiredService<Task<IHSRAssets>>();
                        _hsrAssets = await hsrAssetsTask.ConfigureAwait(false);
                        _hsrDataMapper = _serviceProvider.GetRequiredService<HSRDataMapper>();
                        break;
                    case GameType.ZZZ:
                        var zzzAssetsTask = _serviceProvider.GetRequiredService<Task<IZZZAssets>>();
                        _zzzAssets = await zzzAssetsTask.ConfigureAwait(false);
                        _zzzDataMapper = _serviceProvider.GetRequiredService<ZZZDataMapper>();
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

        private async Task<TResponse> InternalGetRawUserResponseAsync<TResponse>(
            int uid,
            bool bypassCache,
            CancellationToken cancellationToken,
            Func<TResponse, bool> validateResponseContent,
            string missingInfoErrorMessage)
            where TResponse : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureInitializedAsync().ConfigureAwait(false);
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_options.GameType), uid);
            var response = await _httpHelper.Get<TResponse>(endpoint, bypassCache, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve data for UID {uid} for game {_options.GameType}. Response was null.");
            }
            if (!validateResponseContent(response))
            {
                throw new ProfilePrivateException(uid, missingInfoErrorMessage);
            }
            return response;
        }

        private async Task EnsureGameTypeAsync(GameType expectedGameType, string operationDescription)
        {
            await EnsureInitializedAsync().ConfigureAwait(false);
            if (_options.GameType != expectedGameType)
            {
                throw new NotSupportedException(
                    $"This operation ({operationDescription}) is only available for {expectedGameType}. " +
                    $"Current game type: {_options.GameType}");
            }
            switch (expectedGameType)
            {
                case GameType.Genshin:
                    if (_genshinAssets == null || _genshinDataMapper == null)
                        throw new InvalidOperationException($"Genshin Impact assets or data mapper not initialized for operation: {operationDescription}.");
                    break;
                case GameType.HSR:
                    if (_hsrAssets == null || _hsrDataMapper == null)
                        throw new InvalidOperationException($"Honkai: Star Rail assets or data mapper not initialized for operation: {operationDescription}.");
                    break;
                case GameType.ZZZ:
                    if (_zzzAssets == null || _zzzDataMapper == null)
                        throw new InvalidOperationException($"Zenless Zone Zero assets or data mapper not initialized for operation: {operationDescription}.");
                    break;
            }
        }

        public async Task<ApiResponse> GetRawUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAsync(GameType.Genshin, nameof(GetRawUserResponseAsync)).ConfigureAwait(false);
            return await InternalGetRawUserResponseAsync<ApiResponse>(
                uid, bypassCache, cancellationToken,
                response => response.PlayerInfo != null,
                "Profile data retrieved for Genshin Impact, but essential player information is missing. The profile might be private or the UID invalid."
            ).ConfigureAwait(false);
        }

        public async Task<PlayerInfo> GetPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.Genshin, nameof(GetPlayerInfoAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _genshinDataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
        }

        public async Task<IReadOnlyList<Character>> GetCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.Genshin, nameof(GetCharactersAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.PlayerInfo != null && rawResponse.AvatarInfoList == null)
            {
                throw new ProfilePrivateException(uid, $"Character details are hidden for Genshin Impact UID {uid}.");
            }
            if (rawResponse.AvatarInfoList == null)
            {
                return Array.Empty<Character>();
            }
            return _genshinDataMapper.MapCharacters(rawResponse.AvatarInfoList).AsReadOnly();
        }

        public async Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetUserProfileAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.Genshin, nameof(GetUserProfileAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            var playerInfo = _genshinDataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
            IReadOnlyList<Character> characters = Array.Empty<Character>();
            if (rawResponse.AvatarInfoList != null)
            {
                characters = _genshinDataMapper.MapCharacters(rawResponse.AvatarInfoList).AsReadOnly();
            }
            return (playerInfo, characters);
        }

        public async Task<HSRApiResponse> GetRawHSRUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAsync(GameType.HSR, nameof(GetRawHSRUserResponseAsync)).ConfigureAwait(false);
            return await InternalGetRawUserResponseAsync<HSRApiResponse>(
                uid, bypassCache, cancellationToken,
                response => response.DetailInfo != null,
                "Profile data retrieved for Honkai: Star Rail, but detail info is missing. The profile might be private or UID invalid."
            ).ConfigureAwait(false);
        }

        public async Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.HSR, nameof(GetHSRPlayerInfoAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _hsrDataMapper.MapPlayerInfo(rawResponse);
        }

        public async Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.HSR, nameof(GetHSRCharactersAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.DetailInfo?.AvatarDetailList == null)
            {
                return Array.Empty<HSRCharacter>();
            }
            var characters = new List<HSRCharacter>();
            foreach (var avatarDetail in rawResponse.DetailInfo.AvatarDetailList)
            {
                characters.Add(_hsrDataMapper.MapCharacter(avatarDetail));
            }
            return characters.AsReadOnly();
        }

        public async Task<ZZZApiResponse> GetRawZZZUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            await EnsureGameTypeAsync(GameType.ZZZ, nameof(GetRawZZZUserResponseAsync)).ConfigureAwait(false);
            return await InternalGetRawUserResponseAsync<ZZZApiResponse>(
                uid, bypassCache, cancellationToken,
                response => response.PlayerInfo != null,
                "Profile data retrieved for Zenless Zone Zero, but player info is missing. The profile might be private or UID invalid."
            ).ConfigureAwait(false);
        }

        public async Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.ZZZ, nameof(GetZZZPlayerInfoAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _zzzDataMapper.MapPlayerInfo(rawResponse);
        }

        public async Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            await EnsureGameTypeAsync(GameType.ZZZ, nameof(GetZZZAgentsAsync)).ConfigureAwait(false);
            var rawResponse = await GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.PlayerInfo?.ShowcaseDetail?.AvatarList == null)
            {
                return Array.Empty<ZZZAgent>();
            }
            var agents = new List<ZZZAgent>();
            foreach (var avatarModel in rawResponse.PlayerInfo.ShowcaseDetail.AvatarList)
            {
                if (avatarModel != null)
                {
                    var agent = _zzzDataMapper.MapAgent(avatarModel);
                    if (agent != null) agents.Add(agent);
                }
            }
            return agents.AsReadOnly();
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