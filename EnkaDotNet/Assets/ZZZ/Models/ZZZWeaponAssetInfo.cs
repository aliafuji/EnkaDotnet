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
    public class ZZZWeaponAssetInfo
    {
        [JsonProperty("ItemName")]
        public string ItemName { get; set; }

        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("ProfessionType")]
        public string ProfessionType { get; set; }

        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; }

        [JsonProperty("MainStat")]
        public ZZZStatProperty MainStat { get; set; }

        [JsonProperty("SecondaryStat")]
        public ZZZStatProperty SecondaryStat { get; set; }
    }

    public class ZZZStatProperty
    {
        [JsonProperty("PropertyId")]
        public int PropertyId { get; set; }

        [JsonProperty("PropertyValue")]
        public int PropertyValue { get; set; }
    }
}