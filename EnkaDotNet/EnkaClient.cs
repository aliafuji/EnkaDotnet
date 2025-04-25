using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.Genshin;
using EnkaDotNet.Utils.HSR;
using EnkaDotNet.Utils.ZZZ;

namespace EnkaDotNet
{
    public partial class EnkaClient : IDisposable
    {
        private readonly HttpHelper _httpHelper;
        private readonly DataMapper _dataMapper;
        private readonly ZZZDataMapper _zzzDataMapper;
        private readonly HSRDataMapper _hsrDataMapper;
        private readonly IGenshinAssets _genshinAssets;
        private readonly IZZZAssets _zzzAssets;
        private readonly IHSRAssets _hsrAssets;
        private readonly EnkaClientOptions _options;
        private bool _disposed = false;

        public GameType GameType => _options.GameType;
        public EnkaClientOptions Options => _options.Clone();

        public EnkaClient() : this(new EnkaClientOptions()) { }

        public EnkaClient(EnkaClientOptions options = null)
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
                    _dataMapper = new DataMapper(_genshinAssets, _options);
                    break;
                case GameType.ZZZ:
                    _zzzAssets = AssetsFactory.CreateZZZ(_options.Language);
                    _zzzDataMapper = new ZZZDataMapper(_zzzAssets, _options);
                    break;
                case GameType.HSR:
                    _hsrAssets = AssetsFactory.CreateHSR(_options.Language);
                    _hsrDataMapper = new HSRDataMapper(_hsrAssets, _options);
                    break;
                default:
                    throw new UnsupportedGameTypeException(_options.GameType);
            }
        }


        public EnkaClient(HttpHelper httpHelper, IGenshinAssets assets, EnkaClientOptions options = null)
        {
            _options = options ?? new EnkaClientOptions { GameType = GameType.Genshin };
            _options.GameType = GameType.Genshin;
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _genshinAssets = assets ?? throw new ArgumentNullException(nameof(assets));
            _dataMapper = new DataMapper(_genshinAssets, _options);
        }

        public async Task<ApiResponse> GetRawUserResponse(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));
            if (!Constants.IsGameTypeSupported(_options.GameType)) throw new NotSupportedException($"Game type {_options.GameType} is not currently supported.");

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_options.GameType), uid);
            var response = await _httpHelper.Get<ApiResponse>(endpoint, bypassCache, cancellationToken);

            if (response != null && response.AvatarInfoList == null && response.PlayerInfo == null) throw new ProfilePrivateException(uid, "Profile data retrieved but character details and player info are missing, profile might be private or UID invalid.");
            else if (response == null) throw new EnkaNetworkException($"Failed to retrieve data for UID {uid}. Response was null.");

            return response;
        }

        public async Task<PlayerInfo> GetPlayerInfo(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            EnsureGenshinGameType();
            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);
            if (rawResponse?.PlayerInfo == null) throw new EnkaNetworkException($"Failed to retrieve valid PlayerInfo for UID {uid}. Response or PlayerInfo was null.");
            if (_dataMapper == null) throw new InvalidOperationException("Genshin DataMapper is not initialized.");
            return _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
        }

        public async Task<List<Character>> GetCharacters(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            EnsureGenshinGameType();
            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);
            if (rawResponse?.PlayerInfo != null && rawResponse.AvatarInfoList == null) throw new ProfilePrivateException(uid, $"Character details are hidden for UID {uid}.");
            else if (rawResponse?.AvatarInfoList == null) throw new EnkaNetworkException($"Failed to retrieve valid AvatarInfoList for UID {uid}. Response or AvatarInfoList was null.");
            if (_dataMapper == null) throw new InvalidOperationException("Genshin DataMapper is not initialized.");
            return _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
        }

        public async Task<(PlayerInfo PlayerInfo, List<Character> Characters)> GetUserProfile(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            EnsureGenshinGameType();
            var rawResponse = await GetRawUserResponse(uid, bypassCache, cancellationToken);
            if (rawResponse?.PlayerInfo == null) throw new EnkaNetworkException($"Failed to retrieve player info for UID {uid}.");
            if (_dataMapper == null) throw new InvalidOperationException("Genshin DataMapper is not initialized.");
            var playerInfo = _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
            List<Character> characters = new List<Character>();
            if (rawResponse.AvatarInfoList != null) characters = _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
            return (playerInfo, characters);
        }

        public (int Count, int ExpiredCount) GetCacheStats()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            try { return _httpHelper.GetCacheStats(); }
            catch (Exception ex) { throw new NotImplementedException("The underlying HttpHelper does not expose GetCacheStats.", ex); }
        }

        public void ClearCache()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EnkaClient));
            try { _httpHelper.ClearCache(); }
            catch (Exception ex) { throw new NotImplementedException("The underlying HttpHelper does not expose ClearCache.", ex); }
        }

        private void EnsureGenshinGameType()
        {
            if (_options.GameType != GameType.Genshin || _dataMapper == null || _genshinAssets == null)
            {
                throw new NotSupportedException($"This operation is only available for Genshin Impact. Current game type: {_options.GameType}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) { if (disposing) { _httpHelper.Dispose(); } _disposed = true; }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}