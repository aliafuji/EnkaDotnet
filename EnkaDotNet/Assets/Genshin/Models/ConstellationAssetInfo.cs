using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class ConstellationAssetInfo
    {
        [JsonProperty("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonProperty("Icon")]
        public string Icon { get; set; }
    }
}