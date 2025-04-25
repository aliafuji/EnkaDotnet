using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.Genshin
{
    public class FlatDataModel
    {
        [JsonProperty("nameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonProperty("setNameTextMapHash")]
        public string SetNameTextMapHash { get; set; }

        [JsonProperty("rankLevel")]
        public int RankLevel { get; set; }

        [JsonProperty("reliquaryMainstat")]
        public StatPropertyModel ReliquaryMainstat { get; set; }

        [JsonProperty("reliquarySubstats")]
        public List<StatPropertyModel> ReliquarySubstats { get; set; }

        [JsonProperty("weaponStats")]
        public List<StatPropertyModel> WeaponStats { get; set; }

        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("equipType")]
        public string EquipType { get; set; }
    }

    public class StatPropertyModel
    {
        [JsonProperty("mainPropId")]
        public string MainPropId { get; set; }

        [JsonProperty("appendPropId")]
        public string AppendPropId { get; set; }

        [JsonProperty("statValue")]
        public double StatValue { get; set; }
    }
}