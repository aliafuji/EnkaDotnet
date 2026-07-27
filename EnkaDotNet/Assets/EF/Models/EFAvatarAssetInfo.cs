using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFAvatarAssetInfo
    {
        [JsonPropertyName("StrId")]
        public string StrId { get; set; }

        [JsonPropertyName("NameHash")]
        public string NameHash { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("Element")]
        public string Element { get; set; }

        [JsonPropertyName("Profession")]
        public string Profession { get; set; }

        [JsonPropertyName("WeaponType")]
        public string WeaponType { get; set; }

        [JsonPropertyName("MainAttrId")]
        public int MainAttrId { get; set; }

        [JsonPropertyName("SubAttrId")]
        public int SubAttrId { get; set; }

        [JsonPropertyName("AttributeNodes")]
        public Dictionary<string, Dictionary<string, double>> AttributeNodes { get; set; }

        [JsonPropertyName("SkillInfoMap")]
        public Dictionary<string, EFAvatarSkillInfo> SkillInfoMap { get; set; }

        [JsonPropertyName("NodeSkillMap")]
        public Dictionary<string, EFAvatarNodeSkillInfo> NodeSkillMap { get; set; }

        [JsonPropertyName("PotAttributes")]
        public List<EFPotAttributeEntry> PotAttributes { get; set; }

        [JsonPropertyName("BaseHpByLevel")]
        public List<double> BaseHpByLevel { get; set; }

        [JsonPropertyName("BaseAtkByLevel")]
        public List<double> BaseAtkByLevel { get; set; }

        [JsonPropertyName("BaseAttributes")]
        public Dictionary<string, EFBaseAttributeValue> BaseAttributes { get; set; }
    }

    public class EFAvatarSkillInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("Element")]
        public string Element { get; set; }
    }

    public class EFAvatarNodeSkillInfo
    {
        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("Index")]
        public int Index { get; set; }

        [JsonPropertyName("Type")]
        public int Type { get; set; }
    }

    public class EFPotAttributeEntry
    {
        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("Attrs")]
        public Dictionary<string, EFPotAttrValue> Attrs { get; set; }
    }

    public class EFPotAttrValue
    {
        [JsonPropertyName("Value")]
        public double Value { get; set; }

        [JsonPropertyName("Formula")]
        public string Formula { get; set; }
    }

    public class EFBaseAttributeValue
    {
        [JsonPropertyName("BaseValue")]
        public double BaseValue { get; set; }

        [JsonPropertyName("AddValue")]
        public double AddValue { get; set; }
    }
}
