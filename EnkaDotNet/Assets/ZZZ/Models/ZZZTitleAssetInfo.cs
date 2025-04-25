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
    public class ZZZTitleAssetInfo
    {
        [JsonProperty("TitleText")]
        public string TitleText { get; set; }

        [JsonProperty("ColorA")]
        public string ColorA { get; set; }

        [JsonProperty("ColorB")]
        public string ColorB { get; set; }
    }
}