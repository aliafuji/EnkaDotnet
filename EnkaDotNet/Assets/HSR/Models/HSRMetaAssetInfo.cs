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
    public class HSRRelicMetaInfo
    {
        [JsonProperty("mainAffix")]
        public Dictionary<string, Dictionary<string, HSRRelicMainAffixInfo>> MainAffix { get; set; }

        [JsonProperty("subAffix")]
        public Dictionary<string, Dictionary<string, HSRRelicSubAffixInfo>> SubAffix { get; set; }

        [JsonProperty("setSkill")]
        public Dictionary<string, Dictionary<string, HSRSetSkillInfo>> SetSkill { get; set; }
    }

    public class HSRRelicMainAffixInfo
    {
        [JsonProperty("BaseValue")]
        public double BaseValue { get; set; }

        [JsonProperty("LevelAdd")]
        public double LevelAdd { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }
    }

    public class HSRRelicSubAffixInfo
    {
        [JsonProperty("BaseValue")]
        public double BaseValue { get; set; }

        [JsonProperty("Property")]
        public string Property { get; set; }

        [JsonProperty("StepValue")]
        public double StepValue { get; set; }
    }

    public class HSRSetSkillInfo
    {
        [JsonProperty("props")]
        public Dictionary<string, double> Props { get; set; }
    }
}
