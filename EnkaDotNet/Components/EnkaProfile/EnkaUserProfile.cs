using System.Collections.Generic;

namespace EnkaDotNet.Components.EnkaProfile
{
    /// <summary>
    /// Represents an Enka.Network user profile with linked game accounts.
    /// </summary>
    public class EnkaUserProfile
    {
        /// <summary>
        /// The username of the Enka.Network profile.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The unique ID of the Enka.Network profile.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The profile bio/description.
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// The profile level on Enka.Network.
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// The URL of the profile avatar.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// The URL of the profile image.
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// List of linked game accounts (hoyos) with game type identification.
        /// </summary>
        public List<HoyoAccount> HoyoAccounts { get; set; } = new List<HoyoAccount>();
    }
}