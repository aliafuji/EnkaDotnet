using EnkaDotNet.Components.HSR;
using EnkaDotNet.Enums;
using EnkaDotNet.Exceptions;
using EnkaDotNet.Models.HSR;
using EnkaDotNet.Utils;

namespace EnkaDotNet
{
    public partial class EnkaClient
    {
        public async Task<HSRApiResponse?> GetRawHSRUserResponse(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (uid <= 0) throw new ArgumentException("UID must be a positive integer.", nameof(uid));

            EnsureHSRGameType();

            string endpoint = string.Format(Constants.GetUserInfoEndpointFormat(_options.GameType), uid);
            var response = await _httpHelper.Get<HSRApiResponse>(endpoint, bypassCache, cancellationToken);

            if (response != null && response.DetailInfo == null)
            {
                throw new ProfilePrivateException(uid, "Profile data retrieved but detail info is missing, profile might be private or UID invalid.");
            }

            return response;
        }

        public async Task<HSRPlayerInfo> GetHSRPlayerInfo(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureHSRGameType();

            var rawResponse = await GetRawHSRUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve valid PlayerInfo for UID {uid}. Response was null.");
            }

            return _hsrDataMapper!.MapPlayerInfo(rawResponse);
        }

        public async Task<List<HSRCharacter>> GetHSRCharacters(int uid, bool bypassCache = false, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureHSRGameType();

            var rawResponse = await GetRawHSRUserResponse(uid, bypassCache, cancellationToken);

            if (rawResponse?.DetailInfo?.AvatarDetailList == null)
            {
                throw new EnkaNetworkException($"Failed to retrieve character list for UID {uid}. Response or AvatarDetailList was null.");
            }

            var characters = new List<HSRCharacter>();
            foreach (var avatarDetail in rawResponse.DetailInfo.AvatarDetailList)
            {
                characters.Add(_hsrDataMapper!.MapCharacter(avatarDetail));
            }

            return characters;
        }

        private void EnsureHSRGameType()
        {
            if (_options.GameType != GameType.HSR || _hsrDataMapper == null || _hsrAssets == null)
            {
                throw new NotSupportedException(
                    "This operation is only available for Honkai Star Rail. " +
                    $"Current game type: {_options.GameType}");
            }
        }
    }
}