using System;
using EnkaDotNet.Models.EnkaProfile;
using EnkaDotNet.Components.EnkaProfile;

namespace EnkaDotNet.Utils.Enka
{
    public class EnkaDataMapper
    {
        private readonly EnkaClientOptions _options;

        public EnkaDataMapper(EnkaClientOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public EnkaUserProfile MapEnkaUserProfile(EnkaProfileResponse model)
        {
            if (model == null) return null;

            var userProfile = new EnkaUserProfile
            {
                Username = model.Username,
                UserId = model.Id,
            };

            if (model.Profile != null)
            {
                userProfile.Bio = model.Profile.Bio;
                userProfile.Level = model.Profile.Level;
                userProfile.AvatarUrl = model.Profile.Avatar;
                userProfile.ProfileImageUrl = model.Profile.ImageUrl;
            }
            return userProfile;
        }
    }
}
