using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZPfpAssetInfo
    {
        [JsonPropertyName("Icon")]
        public string? Icon { get; set; }
    }
}