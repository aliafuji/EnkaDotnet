using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.HSR
{
    public class HSRAvatarDetail
    {
        [JsonProperty("avatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("promotion")]
        public int Promotion { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("equipment")]
        public HSREquipment Equipment { get; set; }

        [JsonProperty("skillTreeList")]
        public List<HSRSkillTreeModel> SkillTreeList { get; set; }

        [JsonProperty("relicList")]
        public List<HSRRelicModel> RelicList { get; set; }

        [JsonProperty("_assist")]
        public bool IsAssist { get; set; }
    }

    public class HSRSkillTreeModel
    {
        [JsonProperty("pointId")]
        public int PointId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }
    }

    public class HSREquipment
    {
        [JsonProperty("tid")]
        public int Id { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("promotion")]
        public int Promotion { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("_flat")]
        public HSREquipmentFlat Flat { get; set; }
    }

    public class HSREquipmentFlat
    {
        [JsonProperty("props")]
        public List<HSRPropertyInfo> Props { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class HSRPropertyInfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
