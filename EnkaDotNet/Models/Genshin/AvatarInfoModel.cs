using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.Genshin
{
    public class AvatarInfoModel
    {
        [JsonProperty("avatarId")]
        public int AvatarId { get; set; }

        [JsonProperty("propMap")]
        public Dictionary<string, PropValueModel> PropMap { get; set; }

        [JsonProperty("inherentProudSkillId")]
        public int InherentProudSkillId { get; set; }

        [JsonProperty("skillLevelMap")]
        public Dictionary<string, int> SkillLevelMap { get; set; }

        [JsonProperty("equipList")]
        public List<EquipModel> EquipList { get; set; }

        [JsonProperty("fightPropMap")]
        public Dictionary<string, double> FightPropMap { get; set; }

        [JsonProperty("fetterInfo")]
        public FetterInfoModel FetterInfo { get; set; }

        [JsonProperty("costumeId")]
        public int CostumeId { get; set; }

        [JsonProperty("talentIdList")]
        public List<int> TalentIdList { get; set; }

        [JsonProperty("skillDepotId")]
        public int SkillDepotId { get; set; }

        [JsonProperty("proudSkillExtraLevelMap")]
        public Dictionary<string, int> ProudSkillExtraLevelMap { get; set; }
    }

    public class PropValueModel
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("ival")]
        public string Ival { get; set; }

        [JsonProperty("val")]
        public string Val { get; set; }
    }

    public class FetterInfoModel
    {
        [JsonProperty("expLevel")]
        public int ExpLevel { get; set; }
    }
}