using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.ZZZ;
using EnkaDotNet.Utils;

namespace EnkaDotNet
{
    public partial class EnkaClient
    {
        public async Task<ZZZApiResponse?> GetRawZZZUserResponse(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            EnsureZZZGameType();

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_options.GameType), uid);
            var response = await _httpHelper.Get<ZZZApiResponse>(endpoint, bypassCache, cancellationToken);

            if (response != null && response.PlayerInfo == null)
            {
                throw new ProfilePrivateException(uid, "Profile data retrieved but player info is missing, profile might be private or UID invalid.");
            }

            return response;
        }

        public async Task<ZZZPlayerInfo> GetZZZPlayerInfo(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureZZZGameType();

            var rawResponse = await GetRawZZZUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid PlayerInfo for UID {uid}. Response was null.");
            }

            return _zzzDataMapper!.MapPlayerInfo(rawResponse);
        }

        public async Task<List<ZZZAgent>> GetZZZAgents(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureZZZGameType();

            var rawResponse = await GetRawZZZUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse?.PlayerInfo?.ShowcaseDetail?.AvatarList == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve agent list for UID {uid}. Response or AvatarList was null.");
            }

            var agents = new List<ZZZAgent>();
            foreach (var avatarModel in rawResponse.PlayerInfo.ShowcaseDetail.AvatarList)
            {
                agents.Add(_zzzDataMapper!.MapAgent(avatarModel));
            }

            return agents;
        }

        private void EnsureZZZGameType()
        {
            if (_options.GameType != GameType.ZZZ || _zzzDataMapper == null || _zzzAssets == null)
            {
                throw new NotSupportedException(
                    "This operation is only available for Zenless Zone Zero. " +
                    $"Current game type: {_options.GameType}");
            }
        }
    }
}