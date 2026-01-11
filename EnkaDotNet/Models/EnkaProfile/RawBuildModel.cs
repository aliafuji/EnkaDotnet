using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EnkaProfile
{
    /// <summary>
    /// Represents a raw build from the Enka.Network builds API.
    /// Used for deserialization before mapping to game-specific build components.
    /// </summary>
    public class RawBuildModel
    {
        /// <summary>
        /// The unique identifier for this build.
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// The user-defined name for this build.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The display order of this build.
        /// </summary>
        [JsonPropertyName("order")]
        public decimal Order { get; set; }

        /// <summary>
        /// Whether this is the live/active build from the game.
        /// </summary>
        [JsonPropertyName("live")]
        public bool Live { get; set; }

        /// <summary>
        /// The character/agent ID this build belongs to.
        /// </summary>
        [JsonPropertyName("avatar_id")]
        public int AvatarId { get; set; }

        /// <summary>
        /// The raw character/agent data for this build.
        /// </summary>
        [JsonPropertyName("avatar_data")]
        public JsonElement AvatarData { get; set; }
    }
}
