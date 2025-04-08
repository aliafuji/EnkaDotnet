using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Enums;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Utils.ZZZ;
using EnkaDotNet.Assets.ZZZ;

namespace EnkaDotNet
{
    public partial class EnkaClient : IDisposable
    {
        private readonly HttpHelper _httpHelper;
        private readonly DataMapper? _dataMapper;
        private readonly ZZZDataMapper? _zzzDataMapper;
        private readonly IGenshinAssets? _genshinAssets;
        private readonly IZZZAssets? _zzzAssets;
        private readonly EnkaClientOptions _options;
        private bool _disposed = false;

        public GameType GameType => _options.GameType;

        public EnkaClientOptions Options => _options.Clone();

        public EnkaClient() : this(new EnkaClientOptions())
        {
        }

        public EnkaClient(EnkaClientOptions? options = null)
        {
            _options = options ?? new EnkaClientOptions();

            if (!Constants.IsGameTypeSupported(_options.GameType))
            {
                throw new NotSupportedException($"Game type {_options.GameType} is not currently supported.");
            }

            _httpHelper = new HttpHelper(_options.GameType, _options);

            switch (_options.GameType)
            {
                case GameType.Genshin:
                    _genshinAssets = AssetsFactory.CreateGenshin(_options.Language);
                    _dataMapper = new DataMapper(_genshinAssets);
                    break;
                case GameType.ZZZ:
                    _zzzAssets = new ZZZAssets(_options.Language);
                    _zzzDataMapper = new ZZZDataMapper(_zzzAssets);
                    break;
            }
        }

        public EnkaClient(HttpHelper httpHelper, IGenshinAssets assets, EnkaClientOptions? options = null)
        {
            _options = options ?? new EnkaClientOptions { GameType = GameType.Genshin };
            _options.GameType = GameType.Genshin; // Force Genshin game type for this constructor

            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _genshinAssets = assets ?? throw new ArgumentNullException(nameof(assets));
            _dataMapper = new DataMapper(_genshinAssets);
        }

        public async Task<ApiResponse?> GetRawUserResponse(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            // Ensure the game type is supported
            if (!Constants.IsGameTypeSupported(_options.GameType))
            {
                throw new NotSupportedException($"Game type {_options.GameType} is not currently supported.");
            }

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_options.GameType), uid);
            var response = await _httpHelper.Get<ApiResponse>(endpoint, bypassCache, cancellationToken);

            if (response != null && response.AvatarInfoList == null && response.PlayerInfo == null)
            {
                throw new ProfilePrivateException(uid, "Profile data retrieved but character details and player info are missing, profile might be private or UID invalid.");
            }

            return response;
        }

        public async Task<PlayerInfo> GetPlayerInfo(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureGenshinGameType();

            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid PlayerInfo for UID {uid}. Response or PlayerInfo was null.");
            }

            return _dataMapper!.MapPlayerInfo(rawResponse.PlayerInfo);
        }

        public async Task<List<Character>> GetCharacters(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureGenshinGameType();

            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo != null && rawResponse.AvatarInfoList == null)
            {
                throw new ProfilePrivateException(uid, $"Character details are hidden for UID {uid}.");
            }
            else if (rawResponse?.AvatarInfoList == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid AvatarInfoList for UID {uid}. Response or AvatarInfoList was null.");
            }

            return _dataMapper!.MapCharacters(rawResponse.AvatarInfoList);
        }

        public async Task<(PlayerInfo PlayerInfo, List<Character> Characters)> GetUserProfile(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureGenshinGameType();

            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve player info for UID {uid}.");
            }

            var playerInfo = _dataMapper!.MapPlayerInfo(rawResponse.PlayerInfo);

            List<Character> characters = new List<Character>();
            if (rawResponse.AvatarInfoList != null)
            {
                characters = _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
            }

            return (playerInfo, characters);
        }

        public (int Count, int ExpiredCount) GetCacheStats()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            try
            {
                return _httpHelper.GetCacheStats();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException("The underlying HttpHelper does not expose GetCacheStats.", ex);
            }
        }

        public void ClearCache()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            try
            {
                _httpHelper.ClearCache();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException("The underlying HttpHelper does not expose ClearCache.", ex);
            }
        }

        private void EnsureGenshinGameType()
        {
            if (_options.GameType != GameType.Genshin || _dataMapper == null || _genshinAssets == null)
            {
                throw new NotSupportedException(
                    "This operation is only available for Genshin Impact. " +
                    $"Current game type: {_options.GameType}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpHelper.Dispose();
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