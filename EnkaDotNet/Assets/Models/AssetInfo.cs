using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Models
{
    public abstract class AssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string? NameTextMapHash { get; set; }

        [JsonPropertyName("Icon")]
        public string? Icon { get; set; }
    }
}