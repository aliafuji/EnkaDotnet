using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace EnkaDotNet.Models.HSR
{
    public class HSRAvatarDetail
    {
        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("promotion")]
        public int Promotion { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("pos")]
        public int Position { get; set; }

        [JsonPropertyName("equipment")]
        public HSREquipment? Equipment { get; set; }

        [JsonPropertyName("skillTreeList")]
        public List<HSRSkillTreeModel>? SkillTreeList { get; set; }

        [JsonPropertyName("relicList")]
        public List<HSRRelicModel>? RelicList { get; set; }

        [JsonPropertyName("_assist")]
        public bool IsAssist { get; set; }
    }

    public class HSRSkillTreeModel
    {
        [JsonPropertyName("pointId")]
        public int PointId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }
    }

    public class HSREquipment
    {
        [JsonPropertyName("tid")]
        public int Id { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("promotion")]
        public int Promotion { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("_flat")]
        public HSREquipmentFlat? Flat { get; set; }
    }

    public class HSREquipmentFlat
    {
        [JsonPropertyName("props")]
        public List<HSRPropertyInfo>? Props { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class HSRPropertyInfo
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }
    }
}