using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.HSR;
using EnkaDotNet.Components.HSR;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.HSR;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Internal
{
    internal class HSRServiceHandler
    {
        private readonly IHSRAssets _assets;
        private readonly HSRDataMapper _dataMapper;
        private readonly EnkaClientOptions _options;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger _logger;

        public HSRServiceHandler(IHSRAssets assets, EnkaClientOptions options, IHttpHelper httpHelper, ILogger logger)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataMapper = new HSRDataMapper(assets, options);
        }

        public async Task<HSRApiResponse> GetRawHSRUserResponseAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer", nameof(uid));

            string relativePath = string.Format(Constants.DEFAULT_GAME_SPECIFIC_USER_INFO_ENDPOINT_FORMAT, uid);
            string endpoint = $"hsr/{relativePath}";

            HSRApiResponse response = await _httpHelper.Get<HSRApiResponse>(endpoint, bypassCache, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogWarning("API for HSR UID {Uid} returned a successful HTTP status but with empty or null-deserializing content", uid);
                throw new PlayerNotFoundException(uid, $"API for HSR UID {uid} returned a successful HTTP status but with no parsable content or essential data structures The profile may not exist or is not public");
            }

            if (response.DetailInfo == null)
            {
                throw new ProfilePrivateException(uid, $"Profile data retrieved for HSR UID {uid}, but essential detail information (DetailInfo block) is missing The profile might be private, the UID invalid, or an unexpected API response structure was received");
            }
            return response;
        }

        public async Task<HSRPlayerInfo> GetHSRPlayerInfoAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _dataMapper.MapPlayerInfo(rawResponse);
        }

        public async Task<IReadOnlyList<HSRCharacter>> GetHSRCharactersAsync(int uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawHSRUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.DetailInfo?.AvatarDetailList == null)
            {
                _logger.LogInformation("HSR UID {Uid} has public profile info but no character showcase data (AvatarDetailList is null)", uid);
                return Array.Empty<HSRCharacter>();
            }
            var characters = new List<HSRCharacter>();
            foreach (var avatarDetail in rawResponse.DetailInfo.AvatarDetailList)
            {
                characters.Add(_dataMapper.MapCharacter(avatarDetail));
            }
            return characters.AsReadOnly();
        }
    }
}
