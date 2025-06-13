using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class NameCardAssetInfo
    {
        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }
}