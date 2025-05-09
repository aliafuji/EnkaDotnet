using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class WeaponAssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("WeaponType")]
        public string WeaponType { get; set; }

        [JsonPropertyName("RankLevel")]
        public int RankLevel { get; set; }
    }
}