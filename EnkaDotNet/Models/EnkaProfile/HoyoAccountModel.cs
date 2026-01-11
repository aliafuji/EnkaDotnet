using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EnkaProfile
{
    /// <summary>
    /// Represents a linked game account (hoyo) within an Enka.Network profile.
    /// </summary>
    public class HoyoAccountModel
    {
        /// <summary>
        /// The unique hash identifier for this hoyo account.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// The in-game UID for this account.
        /// </summary>
        [JsonPropertyName("uid")]
        public long Uid { get; set; }

        /// <summary>
        /// The player info data (game-specific structure).
        /// </summary>
        [JsonPropertyName("player_info")]
        public JsonElement? PlayerInfo { get; set; }

        /// <summary>
        /// Whether this account has been verified.
        /// </summary>
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        /// <summary>
        /// Whether this account's builds are public.
        /// </summary>
        [JsonPropertyName("public")]
        public bool Public { get; set; }

        /// <summary>
        /// The region/server for this account.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }

        /// <summary>
        /// The order/priority of this hoyo in the profile.
        /// </summary>
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}
