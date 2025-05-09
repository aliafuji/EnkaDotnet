using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class TalentAssetInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("nameTextMapHash")]
        public JsonElement NameTextMapHash { get; set; }
    }
}