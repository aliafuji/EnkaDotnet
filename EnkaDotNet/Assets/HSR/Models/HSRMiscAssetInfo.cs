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
    public class HSRNameCardAssetInfo
    {
        [JsonProperty("Icon")]
        public string Icon { get; set; }
    }

    public class HSRPfpAssetInfo
    {
        [JsonProperty("Icon")]
        public string Icon { get; set; }
    }

    public class HSRPropertyAssetInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Format")]
        public string Format { get; set; }
    }

    public class HSRSkillAssetInfo
    {
        [JsonProperty("IconPath")]
        public string IconPath { get; set; }
    }

    public class HSREidolonAssetInfo
    {
        [JsonProperty("IconPath")]
        public string IconPath { get; set; }

        [JsonProperty("SkillAddLevelList")]
        public Dictionary<string, int> SkillAddLevelList { get; set; }
    }
}
