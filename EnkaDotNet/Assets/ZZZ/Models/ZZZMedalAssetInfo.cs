using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZMedalAssetInfo
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Icon")]
        public string Icon { get; set; }

        [JsonProperty("TipNum")]
        public string TipNum { get; set; }
    }
}