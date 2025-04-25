using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Models.Genshin
{
    public class EquipModel
    {
        [JsonProperty("itemId")]
        public int ItemId { get; set; }

        [JsonProperty("weapon")]
        public WeaponModel Weapon { get; set; }

        [JsonProperty("reliquary")]
        public ReliquaryModel Reliquary { get; set; }

        [JsonProperty("flat")]
        public FlatDataModel Flat { get; set; }
    }

    public class WeaponModel
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("promoteLevel")]
        public int PromoteLevel { get; set; }

        [JsonProperty("affixMap")]
        public Dictionary<string, int> AffixMap { get; set; }
    }

    public class ReliquaryModel
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("mainPropId")]
        public int MainPropId { get; set; }

        [JsonProperty("appendPropIdList")]
        public List<int> AppendPropIdList { get; set; }
    }
}