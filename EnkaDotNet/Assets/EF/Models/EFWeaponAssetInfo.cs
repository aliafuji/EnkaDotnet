using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFWeaponAssetInfo
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("NameHash")]
        public string NameHash { get; set; }

        [JsonPropertyName("WeaponType")]
        public string WeaponType { get; set; }

        [JsonPropertyName("Icon")]
        public string Icon { get; set; }

        [JsonPropertyName("LevelTemplateId")]
        public string LevelTemplateId { get; set; }

        [JsonPropertyName("BreakthroughTemplateId")]
        public string BreakthroughTemplateId { get; set; }

        [JsonPropertyName("TalentTemplateId")]
        public string TalentTemplateId { get; set; }

        [JsonPropertyName("SkillList")]
        public List<int> SkillList { get; set; }
    }
}
