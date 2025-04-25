using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class ArtifactAssetInfo
    {
        [JsonProperty("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonProperty("setIcon")]
        public int SetIcon { get; set; }

        [JsonProperty("EquipType")]
        public string EquipType { get; set; }

        [JsonProperty("Icon")]
        public string Icon { get; set; }

        [JsonProperty("RankLevel")]
        public int RankLevel { get; set; }
    }
}