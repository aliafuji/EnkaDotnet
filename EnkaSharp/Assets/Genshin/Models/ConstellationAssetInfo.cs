using System.Text.Json.Serialization;

namespace EnkaSharp.Assets.Genshin.Models
{
    public class ConstellationAssetInfo
    {
        [JsonPropertyName("NameTextMapHash")]
        public string? NameTextMapHash { get; set; }
        [JsonPropertyName("Icon")]
        public string? Icon { get; set; }
        [JsonPropertyName("DescTextMapHash")]
        public string? DescTextMapHash { get; set; }
        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}