using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.Genshin
{
    public class FlatDataModel
    {
        [JsonPropertyName("nameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonPropertyName("setNameTextMapHash")]
        public string SetNameTextMapHash { get; set; }

        [JsonPropertyName("rankLevel")]
        public int RankLevel { get; set; }

        [JsonPropertyName("reliquaryMainstat")]
        public StatPropertyModel ReliquaryMainstat { get; set; }

        [JsonPropertyName("reliquarySubstats")]
        public List<StatPropertyModel> ReliquarySubstats { get; set; }

        [JsonPropertyName("weaponStats")]
        public List<StatPropertyModel> WeaponStats { get; set; }

        [JsonPropertyName("itemType")]
        public string ItemType { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("equipType")]
        public string EquipType { get; set; }
    }

    public class StatPropertyModel
    {
        [JsonPropertyName("mainPropId")]
        public string MainPropId { get; set; }

        [JsonPropertyName("appendPropId")]
        public string AppendPropId { get; set; }

        [JsonPropertyName("statValue")]
        public double StatValue { get; set; }
    }
}