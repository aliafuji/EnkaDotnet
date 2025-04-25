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
    public class ZZZEquipmentLevelData
    {
        [JsonProperty("Items")]
        public List<ZZZEquipmentLevelItem> Items { get; set; }
    }

    public class ZZZEquipmentLevelItem
    {
        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("EnhanceRate")]
        public double EnhanceRate { get; set; }

        [JsonProperty("Exp")]
        public int Exp { get; set; }

        [JsonProperty("ExpRecycleRate")]
        public int ExpRecycleRate { get; set; }
    }
}