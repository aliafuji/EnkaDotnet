using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class AffixAssetInfo
    {
        [JsonPropertyName("propType")]
        public string? PropType { get; set; }

        [JsonPropertyName("efficiency")]
        public double Efficiency { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }
    }
}