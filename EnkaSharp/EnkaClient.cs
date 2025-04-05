using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaSharp.Assets;
using EnkaSharp.Assets.Genshin;
using EnkaSharp.Components.Genshin;
using EnkaSharp.Enums;
using EnkaSharp.Exceptions;
using EnkaSharp.Models.Genshin;
using EnkaSharp.Utils.Common;
using EnkaSharp.Utils.Genshin;

namespace EnkaSharp
{
    public class EnkaClient : IDisposable
    {
        private readonly HttpHelper _httpHelper;
        private readonly DataMapper _dataMapper;
        private readonly IGenshinAssets _assets;
        private readonly GameType _gameType;
        private bool _disposed = false;

        public EnkaClient(string assetsBasePath, GameType gameType = GameType.Genshin, string language = "en", string? customUserAgent = null)
        {
            _gameType = gameType;
            ValidateGameType(gameType);

            _assets = (IGenshinAssets)AssetsFactory.Create(assetsBasePath, language, gameType);

            _httpHelper = new HttpHelper(gameType, customUserAgent);
            _dataMapper = new DataMapper(_assets);
        }

        public EnkaClient(HttpHelper httpHelper, IGenshinAssets assets, GameType gameType = GameType.Genshin)
        {
            _gameType = gameType;
            ValidateGameType(gameType);
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _dataMapper = new DataMapper(_assets);
        }

        public EnkaClient(HttpHelper httpHelper, IGenshinAssets assets, DataMapper dataMapper, GameType gameType = GameType.Genshin)
        {
            _gameType = gameType;
            ValidateGameType(gameType);
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
        }

        private void ValidateGameType(GameType gameType)
        {
            if (gameType != GameType.Genshin)
            {
                throw new UnsupportedGameTypeException(gameType);
            }
        }

        public async Task<ApiResponse?> GetRawUserResponseAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_gameType), uid);
            var response = await _httpHelper.GetAsync<ApiResponse>(endpoint, bypassCache, cancellationToken);

            if (response != null && response.AvatarInfoList == null && response.PlayerInfo == null)
            {
                throw new ProfilePrivateException(uid, "Profile data retrieved but character details and player info are missing, profile might be private or UID invalid.");
            }


            return response;
        }

        public async Task<PlayerInfo> GetPlayerInfoAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid PlayerInfo for UID {uid}. Response or PlayerInfo was null.");
            }

            return _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
        }

        public async Task<List<Character>> GetCharactersAsync(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo != null && rawResponse.AvatarInfoList == null)
            {
                throw new ProfilePrivateException(uid, $"Character details are hidden for UID {uid}.");
            }
            else if (rawResponse?.AvatarInfoList == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid AvatarInfoList for UID {uid}. Response or AvatarInfoList was null.");
            }


            return _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
        }

        public async Task<(PlayerInfo PlayerInfo, List<Character> Characters)> GetUserProfile(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve player info for UID {uid}.");
            }

            var playerInfo = _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);

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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    (_httpHelper as IDisposable)?.Dispose();
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
