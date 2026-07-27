using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.EF;
using EnkaDotNet.Components.EF;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.EF;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.Common;
using EnkaDotNet.Utils.EF;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Internal
{
    internal class EFServiceHandler
    {
        private readonly IEFAssets _assets;
        private readonly EFDataMapper _dataMapper;
        private readonly EnkaClientOptions _options;
        private readonly IHttpHelper _httpHelper;
        private readonly ILogger _logger;

        public EFServiceHandler(IEFAssets assets, EnkaClientOptions options, IHttpHelper httpHelper, ILogger logger)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataMapper = new EFDataMapper(assets, options);
        }

        public async Task<EFApiResponse> GetRawEFUserResponseAsync(long uid, bool bypassCache, CancellationToken cancellationToken)
        {
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer", nameof(uid));

            string endpoint = $"ef/uid/{uid}";

            EFApiResponse response = await _httpHelper.Get<EFApiResponse>(endpoint, bypassCache, cancellationToken).ConfigureAwait(false);

            if (response == null)
            {
                _logger.LogWarning("API for Endfield UID {Uid} returned a successful HTTP status but with empty or null-deserializing content", uid);
                throw new PlayerNotFoundException(uid, $"API for Endfield UID {uid} returned a successful HTTP status but with no parsable content or essential data structures The profile may not exist or is not public");
            }

            if (response.PlayerInfo == null)
            {
                throw new ProfilePrivateException(uid, $"Profile data retrieved for Endfield UID {uid}, but essential player information (PlayerInfo block) is missing The profile might be private, the UID invalid, or an unexpected API response structure was received");
            }

            return response;
        }

        public async Task<EFPlayerInfo> GetEFPlayerInfoAsync(long uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawEFUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            return _dataMapper.MapPlayerInfo(rawResponse);
        }

        public async Task<IReadOnlyList<EFOperator>> GetEFOperatorsAsync(long uid, bool bypassCache, CancellationToken cancellationToken)
        {
            var rawResponse = await GetRawEFUserResponseAsync(uid, bypassCache, cancellationToken).ConfigureAwait(false);
            if (rawResponse.PlayerInfo?.CharData == null)
            {
                _logger.LogInformation("Endfield UID {Uid} has public profile info but no operator showcase data (CharData is null)", uid);
                return Array.Empty<EFOperator>();
            }

            var operators = new List<EFOperator>();
            foreach (var charData in rawResponse.PlayerInfo.CharData)
            {
                if (charData == null) continue;
                var op = _dataMapper.MapOperator(charData);
                if (op != null) operators.Add(op);
            }

            return operators.AsReadOnly();
        }
    }
}
