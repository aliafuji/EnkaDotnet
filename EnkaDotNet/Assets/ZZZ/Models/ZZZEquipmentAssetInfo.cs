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
    public class ZZZEquipmentData
    {
        [JsonProperty("Items")]
        public Dictionary<string, ZZZEquipmentItemInfo> Items { get; set; }

        [JsonProperty("Suits")]
        public Dictionary<string, ZZZEquipmentSuitInfo> Suits { get; set; }
    }

    public class ZZZEquipmentItemInfo
    {
        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("SuitId")]
        public int SuitId { get; set; }
    }

    public class ZZZEquipmentSuitInfo
    {
        [JsonProperty("Icon")]
        public string Icon { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("SetBonusProps")]
        public Dictionary<string, int> SetBonusProps { get; set; }
    }
}