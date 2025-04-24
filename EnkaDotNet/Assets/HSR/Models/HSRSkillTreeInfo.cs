using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRSkillTreeData
    {
        [JsonPropertyName("0")]
        public List<string>? BasicSkills { get; set; }

        [JsonPropertyName("1")]
        public List<List<string>>? MinorTraces { get; set; }

        [JsonPropertyName("2")]
        public List<List<string>>? MajorTraces { get; set; }
    }

    public class HSRSkillTreePointInfo
    {
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        
        [JsonPropertyName("pointType")]
        public int PointType { get; set; }
        
        [JsonPropertyName("anchor")]
        public string? Anchor { get; set; }
        
        [JsonPropertyName("maxLevel")]
        public int MaxLevel { get; set; }
        
        [JsonPropertyName("skillIds")]
        public List<int>? SkillIds { get; set; }
    }
}