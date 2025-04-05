using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class ConstellationAssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string? NameTextMapHash { get; set; }

        [JsonPropertyName("Icon")]
        public string? Icon { get; set; }
    }
}