using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.Genshin
{
    public class EquipModel
    {
        [JsonPropertyName("itemId")]
        public int ItemId { get; set; }

        [JsonPropertyName("weapon")]
        public WeaponModel? Weapon { get; set; }

        [JsonPropertyName("reliquary")]
        public ReliquaryModel? Reliquary { get; set; }

        [JsonPropertyName("flat")]
        public FlatDataModel? Flat { get; set; }
    }

    public class WeaponModel
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("promoteLevel")]
        public int PromoteLevel { get; set; }

        [JsonPropertyName("affixMap")]
        public Dictionary<string, int>? AffixMap { get; set; }
    }

    public class ReliquaryModel
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("mainPropId")]
        public int MainPropId { get; set; }

        [JsonPropertyName("appendPropIdList")]
        public List<int>? AppendPropIdList { get; set; }
    }

}
