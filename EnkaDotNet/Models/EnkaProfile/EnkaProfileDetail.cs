using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EnkaProfile
{
    public class EnkaProfileDetail
    {
        [JsonPropertyName("bio")]
        public string Bio { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }
    }
}