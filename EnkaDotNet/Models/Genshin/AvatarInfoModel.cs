using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.Genshin
{
    public class AvatarInfoModel
    {
        [JsonPropertyName("avatarId")]
        public int AvatarId { get; set; }

        [JsonPropertyName("propMap")]
        public Dictionary<string, PropValueModel>? PropMap { get; set; }

        [JsonPropertyName("inherentProudSkillId")]
        public int InherentProudSkillId { get; set; }

        [JsonPropertyName("skillLevelMap")]
        public Dictionary<string, int>? SkillLevelMap { get; set; }

        [JsonPropertyName("equipList")]
        public List<EquipModel>? EquipList { get; set; }

        [JsonPropertyName("fightPropMap")]
        public Dictionary<string, double>? FightPropMap { get; set; }

        [JsonPropertyName("fetterInfo")]
        public FetterInfoModel? FetterInfo { get; set; }

        [JsonPropertyName("costumeId")]
        public int? CostumeId { get; set; }

        [JsonPropertyName("talentIdList")]
        public List<int>? TalentIdList { get; set; }

        [JsonPropertyName("skillDepotId")]
        public int SkillDepotId { get; set; }

        [JsonPropertyName("proudSkillExtraLevelMap")]
        public Dictionary<string, int>? ProudSkillExtraLevelMap { get; set; }
    }

    public class PropValueModel
    {
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("ival")]
        public string? Ival { get; set; }

        [JsonPropertyName("val")]
        public string? Val { get; set; }
    }

    public class FetterInfoModel
    {
        [JsonPropertyName("expLevel")]
        public int ExpLevel { get; set; }
    }
}
