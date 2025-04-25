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
    public class TalentAssetInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonProperty("DescTextMapHash")]
        public string DescTextMapHash { get; set; }
    }
}