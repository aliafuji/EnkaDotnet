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
    public class HSRSkillTreeData
    {
        [JsonProperty("0")]
        public List<string> BasicSkills { get; set; }

        [JsonProperty("1")]
        public List<List<string>> MinorTraces { get; set; }

        [JsonProperty("2")]
        public List<List<string>> MajorTraces { get; set; }
    }

    public class HSRSkillTreePointInfo
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("pointType")]
        public int PointType { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }

        [JsonProperty("maxLevel")]
        public int MaxLevel { get; set; }

        [JsonProperty("skillIds")]
        public List<int> SkillIds { get; set; }
    }
}
