using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Components.Genshin;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.Genshin;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.Genshin;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Internal
{
    internal class GenshinServiceHandler
    {
        private readonly IGenshinAssets _assets;
        private readonly DataMapper _dataMapper;
        private readonly EnkaClientOptions _options;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger _logger;

        public GenshinServiceHandler(IGenshinAssets assets, EnkaClientOptions options, IHttpHelper httpHelper, ILogger logger)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataMapper = new DataMapper(assets, options);
        }

        public async Task<ApiResponse> GetRawUserResponseAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer", nameof(uid));

            string endpoint = string.Format(Constants.DEFAULT_GAME_SPECIFIC_USER_INFO_ENDPOINT_FORMAT, uid);

            ApiResponse response = await _httpHelper.Get<ApiResponse>(endpoint, bypassCache, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogWarning("API for Genshin UID {Uid} returned a successful HTTP status but with empty or null-deserializing content", uid);
                throw new PlayerNotFoundException(uid, $"API for Genshin UID {uid} returned a successful HTTP status but with no parsable content or essential data structures The profile may not exist or is not public");
            }

            if (response.PlayerInfo == null)
            {
                if (response.AvatarInfoList == null && response.Ttl == -1)
                {
                    throw new ProfilePrivateException(uid, $"Profile data for Genshin Impact UID {uid} appears to be private (eg TTL -1 and no avatar data)");
                }
                throw new ProfilePrivateException(uid, $"Profile data retrieved for Genshin Impact UID {uid}, but essential player information (PlayerInfo block) is missing The profile might be private, the UID invalid, or an unexpected API response structure was received");
            }
            return response;
        }

        public async Task<PlayerInfo> GetPlayerInfoAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            var playerInfo = _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
            playerInfo.Uid = rawResponse.Uid;
            playerInfo.TTL = rawResponse.Ttl.ToString();
            return playerInfo;
        }

        public async Task<IReadOnlyList<Character>> GetCharactersAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.AvatarInfoList == null)
            {
                _logger.LogInformation("Genshin UID {Uid} has public profile info but no character showcase data (AvatarInfoList is null)", uid);
                return Array.Empty<Character>();
            }

            var mappedCharacters = _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
            var characterList = new List<Character>();
            foreach (var character in mappedCharacters)
            {
                characterList.Add(character);
            }
            return characterList;
        }

        public async Task<(PlayerInfo PlayerInfo, IReadOnlyList<Character> Characters)> GetUserProfileAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            var playerInfo = _dataMapper.MapPlayerInfo(rawResponse.PlayerInfo);
            playerInfo.Uid = rawResponse.Uid;
            playerInfo.TTL = rawResponse.Ttl.ToString();

            IReadOnlyList<Character> characters;
            if (rawResponse.AvatarInfoList != null)
            {
                var mappedCharacters = _dataMapper.MapCharacters(rawResponse.AvatarInfoList);
                var characterList = new List<Character>();
                foreach (var character in mappedCharacters)
                {
                    characterList.Add(character);
                }
                characters = characterList;
            }
            else
            {
                characters = Array.Empty<Character>();
                _logger.LogInformation("Genshin UID {Uid} has public profile info but no character showcase data (AvatarInfoList is null) for GetUserProfileAsync", uid);
            }
            return (playerInfo, characters);
        }
    }
}
