using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFEquipData
    {
        [JsonPropertyName("Items")]
        public Dictionary<string, EFEquipItemInfo> Items { get; set; }

        [JsonPropertyName("Suits")]
        public Dictionary<string, EFEquipSuitInfo> Suits { get; set; }
    }

    public class EFEquipItemInfo
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("SuitId")]
        public string SuitId { get; set; }

        [JsonPropertyName("AttrModifiers")]
        public List<EFAttrModifierInfo> AttrModifiers { get; set; }
    }

    public class EFEquipSuitInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("NameHash")]
        public string NameHash { get; set; }

        [JsonPropertyName("SkillId")]
        public int SkillId { get; set; }
    }

    public class EFAttrModifierInfo
    {
        [JsonPropertyName("AttrType")]
        public int AttrType { get; set; }

        [JsonPropertyName("Formula")]
        public string Formula { get; set; }

        [JsonPropertyName("Values")]
        public List<double> Values { get; set; }
    }
}
