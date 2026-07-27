using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.EF.Models
{
    public class EFWeaponMetaData
    {
        [JsonPropertyName("BreakSkillLevelBounds")]
        public Dictionary<string, Dictionary<string, List<EFSkillLevelBound>>> BreakSkillLevelBounds { get; set; }

        [JsonPropertyName("LevelCurves")]
        public Dictionary<string, List<double>> LevelCurves { get; set; }

        [JsonPropertyName("TalentSkillLevelBounds")]
        public Dictionary<string, Dictionary<string, List<EFSkillLevelBound>>> TalentSkillLevelBounds { get; set; }
    }

    public class EFSkillLevelBound
    {
        [JsonPropertyName("lowerBound")]
        public int LowerBound { get; set; }

        [JsonPropertyName("upperBound")]
        public int UpperBound { get; set; }
    }
}
