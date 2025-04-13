using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRRelicMetaInfo
    {
        [JsonPropertyName("mainAffix")]
        public Dictionary<string, Dictionary<string, HSRRelicMainAffixInfo>>? MainAffix { get; set; }

        [JsonPropertyName("subAffix")]
        public Dictionary<string, Dictionary<string, HSRRelicSubAffixInfo>>? SubAffix { get; set; }

        [JsonPropertyName("setSkill")]
        public Dictionary<string, Dictionary<string, HSRSetSkillInfo>>? SetSkill { get; set; }
    }

    public class HSRRelicMainAffixInfo
    {
        [JsonPropertyName("BaseValue")]
        public double BaseValue { get; set; }

        [JsonPropertyName("LevelAdd")]
        public double LevelAdd { get; set; }

        [JsonPropertyName("Property")]
        public string? Property { get; set; }
    }

    public class HSRRelicSubAffixInfo
    {
        [JsonPropertyName("BaseValue")]
        public double BaseValue { get; set; }

        [JsonPropertyName("Property")]
        public string? Property { get; set; }

        [JsonPropertyName("StepValue")]
        public double StepValue { get; set; }
    }

    public class HSRSetSkillInfo
    {
        [JsonPropertyName("props")]
        public Dictionary<string, double>? Props { get; set; }
    }
}