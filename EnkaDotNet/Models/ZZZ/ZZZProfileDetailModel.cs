using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZProfileDetailModel
    {
        [JsonPropertyName("Nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("AvatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("Uid")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long Uid { get; set; }  // Changed from string to long to handle numeric UIDs

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("Title")]
        public int Title { get; set; }

        [JsonPropertyName("ProfileId")]
        public int ProfileId { get; set; }

        [JsonPropertyName("PlatformType")]
        public int PlatformType { get; set; }

        [JsonPropertyName("CallingCardId")]
        public int CallingCardId { get; set; }
    }
}