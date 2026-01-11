using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EnkaProfile
{
    /// <summary>
    /// Represents the API response for an Enka.Network profile.
    /// </summary>
    public class EnkaProfileResponse
    {
        /// <summary>
        /// The username of the Enka.Network profile.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// The profile details (bio, level, avatar, etc.).
        /// </summary>
        [JsonPropertyName("profile")]
        public EnkaProfileDetail Profile { get; set; }

        /// <summary>
        /// The unique ID of the Enka.Network profile.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// Dictionary of linked game accounts, keyed by hash.
        /// </summary>
        [JsonPropertyName("hoyos")]
        public Dictionary<string, HoyoAccountModel> Hoyos { get; set; }
    }
}