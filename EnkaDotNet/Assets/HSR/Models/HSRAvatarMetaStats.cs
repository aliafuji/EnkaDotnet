using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRAvatarMetaStats
    {
        [JsonProperty("AttackAdd")]
        public double AttackAdd { get; set; }

        [JsonProperty("AttackBase")]
        public double AttackBase { get; set; }

        [JsonProperty("BaseAggro")]
        public double BaseAggro { get; set; }

        [JsonProperty("CriticalChance")]
        public double CriticalChance { get; set; }

        [JsonProperty("CriticalDamage")]
        public double CriticalDamage { get; set; }

        [JsonProperty("DefenceAdd")]
        public double DefenceAdd { get; set; }

        [JsonProperty("DefenceBase")]
        public double DefenceBase { get; set; }

        [JsonProperty("HPAdd")]
        public double HPAdd { get; set; }

        [JsonProperty("HPBase")]
        public double HPBase { get; set; }

        [JsonProperty("SpeedBase")]
        public double SpeedBase { get; set; }
    }

    public class HSREquipmentMetaStats
    {
        [JsonProperty("AttackAdd")]
        public double AttackAdd { get; set; }

        [JsonProperty("BaseAttack")]
        public double BaseAttack { get; set; }

        [JsonProperty("BaseDefence")]
        public double BaseDefence { get; set; }

        [JsonProperty("BaseHP")]
        public double BaseHP { get; set; }

        [JsonProperty("DefenceAdd")]
        public double DefenceAdd { get; set; }

        [JsonProperty("HPAdd")]
        public double HPAdd { get; set; }
    }

    public class HSRMetaData
    {
        [JsonProperty("avatar")]
        public Dictionary<string, Dictionary<string, HSRAvatarMetaStats>> AvatarStats { get; set; }

        [JsonProperty("equipment")]
        public Dictionary<string, Dictionary<string, HSREquipmentMetaStats>> EquipmentStats { get; set; }

        [JsonProperty("equipmentSkill")]
        public Dictionary<string, Dictionary<string, HSREquipmentSkillInfo>> EquipmentSkills { get; set; }

        [JsonProperty("relic")]
        public HSRRelicMetaInfo RelicInfo { get; set; }

        [JsonProperty("tree")]
        public Dictionary<string, Dictionary<string, HSRSkillTreeMetaInfo>> SkillTreeInfo { get; set; }
    }

    public class HSREquipmentSkillInfo
    {
        [JsonProperty("props")]
        public Dictionary<string, double> Props { get; set; }
    }

    public class HSRSkillTreeMetaInfo
    {
        [JsonProperty("props")]
        public Dictionary<string, double> Props { get; set; }
    }
}
