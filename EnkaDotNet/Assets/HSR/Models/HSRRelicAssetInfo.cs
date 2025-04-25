using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRRelicData
    {
        [JsonProperty("Items")]
        public Dictionary<string, HSRRelicItemInfo> Items { get; set; }

        [JsonProperty("Sets")]
        public Dictionary<string, HSRRelicSetInfo> Sets { get; set; }
    }

    public class HSRRelicItemInfo
    {
        [JsonProperty("Icon")]
        public string Icon { get; set; }

        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("SetID")]
        public int SetID { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("MainAffixGroup")]
        public int MainAffixGroup { get; set; }

        [JsonProperty("SubAffixGroup")]
        public int SubAffixGroup { get; set; }
    }

    public class HSRRelicSetInfo
    {
        [JsonProperty("SetName")]
        public string SetName { get; set; }

        [JsonProperty("SetEffects")]
        public Dictionary<string, HSRSetEffectInfo> SetEffects { get; set; }
    }

    public class HSRSetEffectInfo
    {
        [JsonProperty("EffectDesc")]
        public string EffectDesc { get; set; }

        [JsonProperty("Properties")]
        public Dictionary<string, double> Properties { get; set; }
    }
}
