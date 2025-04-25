using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.HSR
{
    public class HSRRelicModel
    {
        [JsonProperty("tid")]
        public int Id { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("mainAffixId")]
        public int MainAffixId { get; set; }

        [JsonProperty("subAffixList")]
        public List<HSRSubAffix> SubAffixList { get; set; }

        [JsonProperty("_flat")]
        public HSRRelicFlat Flat { get; set; }
    }

    public class HSRSubAffix
    {
        [JsonProperty("affixId")]
        public int AffixId { get; set; }

        [JsonProperty("cnt")]
        public int Count { get; set; }

        [JsonProperty("step")]
        public int Step { get; set; }
    }

    public class HSRRelicFlat
    {
        [JsonProperty("props")]
        public List<HSRPropertyInfo> Props { get; set; }

        [JsonProperty("setName")]
        public string SetName { get; set; }

        [JsonProperty("setID")]
        public int SetId { get; set; }
    }
}
