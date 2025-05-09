using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.HSR
{
    public class HSRRelicModel
    {
        [JsonPropertyName("tid")]
        public int Id { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("mainAffixId")]
        public int MainAffixId { get; set; }

        [JsonPropertyName("subAffixList")]
        public List<HSRSubAffix> SubAffixList { get; set; }

        [JsonPropertyName("_flat")]
        public HSRRelicFlat Flat { get; set; }
    }

    public class HSRSubAffix
    {
        [JsonPropertyName("affixId")]
        public int AffixId { get; set; }

        [JsonPropertyName("cnt")]
        public int Count { get; set; }

        [JsonPropertyName("step")]
        public int Step { get; set; }
    }

    public class HSRRelicFlat
    {
        [JsonPropertyName("props")]
        public List<HSRPropertyInfo> Props { get; set; }

        [JsonPropertyName("setName")]
        public string SetName { get; set; }

        [JsonPropertyName("setID")]
        public int SetId { get; set; }
    }
}