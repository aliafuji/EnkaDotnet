using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class ConstellationAssetInfo
    {
        [JsonPropertyName("nameTextMapHash")]
        public JsonElement NameTextMapHash { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }
}