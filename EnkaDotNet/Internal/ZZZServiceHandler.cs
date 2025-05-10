using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.ZZZ;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Internal
{
    internal class ZZZServiceHandler
    {
        private readonly IZZZAssets _assets;
        private readonly ZZZDataMapper _dataMapper;
        private readonly EnkaClientOptions _options;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger _logger;

        public ZZZServiceHandler(IZZZAssets assets, EnkaClientOptions options, IHttpHelper httpHelper, ILogger logger)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataMapper = new ZZZDataMapper(assets, options);
        }

        public async Task<ZZZApiResponse> GetRawZZZUserResponseAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(GameType.ZZZ), uid);
            ZZZApiResponse response = await _httpHelper.Get<ZZZApiResponse>(endpoint, bypassCache, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogWarning("API for ZZZ UID {Uid} returned a successful HTTP status but with empty or null-deserializing content.", uid);
                throw new PlayerNotFoundException(uid, $"API for ZZZ UID {uid} returned a successful HTTP status but with no parsable content or essential data structures. The profile may not exist or is not public.");
            }

            if (response.PlayerInfo == null)
            {
                throw new ProfilePrivateException(uid, $"Profile data retrieved for ZZZ UID {uid}, but essential player information (PlayerInfo block) is missing. The profile might be private, the UID invalid, or an unexpected API response structure was received.");
            }
            return response;
        }

        public async Task<ZZZPlayerInfo> GetZZZPlayerInfoAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _dataMapper.MapPlayerInfo(rawResponse);
        }

        public async Task<IReadOnlyList<ZZZAgent>> GetZZZAgentsAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawZZZUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.PlayerInfo?.ShowcaseDetail?.AvatarList == null)
            {
                _logger.LogInformation("ZZZ UID {Uid} has public profile info but no agent showcase data (AvatarList is null).", uid);
                return Array.Empty<ZZZAgent>();
            }
            var agents = new List<ZZZAgent>();
            foreach (var avatarModel in rawResponse.PlayerInfo.ShowcaseDetail.AvatarList)
            {
                if (avatarModel != null)
                {
                    var agent = _dataMapper.MapAgent(avatarModel);
                    if (agent != null) agents.Add(agent);
                }
            }
            return agents.AsReadOnly();
        }
    }
}
