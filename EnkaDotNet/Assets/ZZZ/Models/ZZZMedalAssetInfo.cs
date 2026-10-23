using System.Text.Json.Serialization;

using System.Collections.Generic;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZMedalAssetInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("TipNum")]
        public List<int> TipNum { get; set; } = new List<int>();

        [JsonPropertyName("PrefixIcon")]
        public string PrefixIcon { get; set; }
    }
}
