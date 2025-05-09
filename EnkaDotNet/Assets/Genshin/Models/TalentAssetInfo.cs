using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class TalentAssetInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonPropertyName("DescTextMapHash")]
        public string DescTextMapHash { get; set; }
    }
}