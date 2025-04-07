using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class PfpAssetInfo
    {
        [JsonPropertyName("iconPath")]
        public string? IconPath { get; set; }
    }
}