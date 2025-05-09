using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRRelicItemInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("SetID")]
        public int SetID { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("MainAffixGroup")]
        public int MainAffixGroup { get; set; }

        [JsonPropertyName("SubAffixGroup")]
        public int SubAffixGroup { get; set; }
    }

    public class HSRRelicSetInfo
    {
        [JsonPropertyName("SetName")]
        public string SetName { get; set; }

        [JsonPropertyName("SetEffects")]
        public Dictionary<string, HSRSetEffectInfo> SetEffects { get; set; }
    }

    public class HSRSetEffectInfo
    {
        [JsonPropertyName("EffectDesc")]
        public string EffectDesc { get; set; }

        [JsonPropertyName("Properties")]
        public Dictionary<string, double> Properties { get; set; }
    }
}