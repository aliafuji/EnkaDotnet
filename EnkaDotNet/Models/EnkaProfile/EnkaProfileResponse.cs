using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EnkaProfile
{
    public class EnkaProfileResponse
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("profile")]
        public EnkaProfileDetail Profile { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}