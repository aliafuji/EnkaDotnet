using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRAvatarMetaStats
    {
        [JsonPropertyName("AttackAdd")]
        public double AttackAdd { get; set; }

        [JsonPropertyName("AttackBase")]
        public double AttackBase { get; set; }

        [JsonPropertyName("BaseAggro")]
        public double BaseAggro { get; set; }

        [JsonPropertyName("CriticalChance")]
        public double CriticalChance { get; set; }

        [JsonPropertyName("CriticalDamage")]
        public double CriticalDamage { get; set; }

        [JsonPropertyName("DefenceAdd")]
        public double DefenceAdd { get; set; }

        [JsonPropertyName("DefenceBase")]
        public double DefenceBase { get; set; }

        [JsonPropertyName("HPAdd")]
        public double HPAdd { get; set; }

        [JsonPropertyName("HPBase")]
        public double HPBase { get; set; }

        [JsonPropertyName("SpeedBase")]
        public double SpeedBase { get; set; }
    }

    public class HSREquipmentMetaStats
    {
        [JsonPropertyName("AttackAdd")]
        public double AttackAdd { get; set; }

        [JsonPropertyName("BaseAttack")]
        public double BaseAttack { get; set; }

        [JsonPropertyName("BaseDefence")]
        public double BaseDefence { get; set; }

        [JsonPropertyName("BaseHP")]
        public double BaseHP { get; set; }

        [JsonPropertyName("DefenceAdd")]
        public double DefenceAdd { get; set; }

        [JsonPropertyName("HPAdd")]
        public double HPAdd { get; set; }
    }

    public class HSRMetaData
    {
        [JsonPropertyName("avatar")]
        public Dictionary<string, Dictionary<string, HSRAvatarMetaStats>>? AvatarStats { get; set; }

        [JsonPropertyName("equipment")]
        public Dictionary<string, Dictionary<string, HSREquipmentMetaStats>>? EquipmentStats { get; set; }

        [JsonPropertyName("equipmentSkill")]
        public Dictionary<string, Dictionary<string, HSREquipmentSkillInfo>>? EquipmentSkills { get; set; }

        [JsonPropertyName("relic")]
        public HSRRelicMetaInfo? RelicInfo { get; set; }

        [JsonPropertyName("tree")]
        public Dictionary<string, Dictionary<string, HSRSkillTreeMetaInfo>>? SkillTreeInfo { get; set; }
    }

    public class HSREquipmentSkillInfo
    {
        [JsonPropertyName("props")]
        public Dictionary<string, double>? Props { get; set; }
    }

    public class HSRSkillTreeMetaInfo
    {
        [JsonPropertyName("props")]
        public Dictionary<string, double>? Props { get; set; }
    }
}