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
    public class ZZZWeaponStarData
    {
        [JsonProperty("Items")]
        public List<ZZZWeaponStarItem> Items { get; set; }
    }

    public class ZZZWeaponStarItem
    {
        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonProperty("StarRate")]
        public double StarRate { get; set; }

        [JsonProperty("RandRate")]
        public double RandRate { get; set; }

        [JsonProperty("UnlockLevel")]
        public int UnlockLevel { get; set; }

        [JsonProperty("Exp")]
        public int Exp { get; set; }
    }
}