using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.EF
{
    public class EFApiResponse
    {
        [JsonPropertyName("playerInfo")]
        public EFPlayerInfoModel PlayerInfo { get; set; }

        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }
    }

    public class EFPlayerInfoModel
    {
        [JsonPropertyName("businessCard")]
        public EFBusinessCardModel BusinessCard { get; set; }

        [JsonPropertyName("charData")]
        public List<EFCharDataModel> CharData { get; set; }
    }

    public class EFBusinessCardModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("shortId")]
        public string ShortId { get; set; }

        [JsonPropertyName("gender")]
        public int Gender { get; set; }

        [JsonPropertyName("businessCardTopicId")]
        public int BusinessCardTopicId { get; set; }

        [JsonPropertyName("signature")]
        public string Signature { get; set; }

        [JsonPropertyName("userAvatarId")]
        public int UserAvatarId { get; set; }

        [JsonPropertyName("userAvatarFrameId")]
        public int UserAvatarFrameId { get; set; }

        [JsonPropertyName("adventureLevel")]
        public int AdventureLevel { get; set; }

        [JsonPropertyName("worldLevel")]
        public int WorldLevel { get; set; }

        [JsonPropertyName("mainMissionId")]
        public string MainMissionId { get; set; }

        [JsonPropertyName("createTime")]
        public long CreateTime { get; set; }

        [JsonPropertyName("platformRoleId")]
        public string PlatformRoleId { get; set; }

        [JsonPropertyName("domainDev")]
        public EFDomainDevModel DomainDev { get; set; }

        [JsonPropertyName("achievement")]
        public EFAchievementModel Achievement { get; set; }

        [JsonPropertyName("statistic")]
        public EFStatisticModel Statistic { get; set; }

        [JsonPropertyName("charList")]
        public List<EFCharListEntryModel> CharList { get; set; }

        [JsonPropertyName("businessCardExpandFlag")]
        public bool BusinessCardExpandFlag { get; set; }
    }

    public class EFDomainDevModel
    {
        [JsonPropertyName("domains")]
        public List<EFDomainModel> Domains { get; set; }
    }

    public class EFDomainModel
    {
        [JsonPropertyName("domainId")]
        public string DomainId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }
    }

    public class EFAchievementModel
    {
        [JsonPropertyName("display")]
        public List<EFIntKeyValueModel> Display { get; set; }

        [JsonPropertyName("infoList")]
        public List<EFAchievementInfoModel> InfoList { get; set; }
    }

    public class EFAchievementInfoModel
    {
        [JsonPropertyName("achieveNumId")]
        public int AchieveNumId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("isPlated")]
        public bool IsPlated { get; set; }
    }

    public class EFStatisticModel
    {
        [JsonPropertyName("charNum")]
        public int CharNum { get; set; }

        [JsonPropertyName("weaponNum")]
        public int WeaponNum { get; set; }

        [JsonPropertyName("docNum")]
        public int DocNum { get; set; }
    }

    public class EFCharListEntryModel
    {
        [JsonPropertyName("templateId")]
        public string TemplateId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("potentialLevel")]
        public int PotentialLevel { get; set; }
    }

    public class EFCharDataModel
    {
        [JsonPropertyName("templateId")]
        public int TemplateId { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("exp")]
        public int Exp { get; set; }

        [JsonPropertyName("potentialLevel")]
        public int PotentialLevel { get; set; }

        [JsonPropertyName("equip")]
        public List<EFEquipEntryModel> Equip { get; set; }

        [JsonPropertyName("weapon")]
        public EFWeaponModel Weapon { get; set; }

        [JsonPropertyName("skillInfo")]
        public EFSkillInfoModel SkillInfo { get; set; }

        [JsonPropertyName("equipMedicineId")]
        public int EquipMedicineId { get; set; }

        [JsonPropertyName("talent")]
        public EFTalentModel Talent { get; set; }

        [JsonPropertyName("potentialCg")]
        public List<object> PotentialCg { get; set; }
    }

    public class EFEquipEntryModel
    {
        [JsonPropertyName("key")]
        public int Key { get; set; }

        [JsonPropertyName("value")]
        public EFEquipValueModel Value { get; set; }
    }

    public class EFEquipValueModel
    {
        [JsonPropertyName("templateid")]
        public int TemplateId { get; set; }

        [JsonPropertyName("enhance")]
        public List<EFIntKeyValueModel> Enhance { get; set; }

        [JsonPropertyName("equipEnhanceData")]
        public EFEquipEnhanceDataModel EquipEnhanceData { get; set; }
    }

    public class EFEquipEnhanceDataModel
    {
        [JsonPropertyName("enhanceFailedDataByAttrIndex")]
        public List<EFEnhanceFailedEntryModel> EnhanceFailedDataByAttrIndex { get; set; }
    }

    public class EFEnhanceFailedEntryModel
    {
        [JsonPropertyName("key")]
        public int Key { get; set; }

        [JsonPropertyName("value")]
        public EFEnhanceFailedValueModel Value { get; set; }
    }

    public class EFEnhanceFailedValueModel
    {
        [JsonPropertyName("enhanceFailedTimesByLevel")]
        public List<object> EnhanceFailedTimesByLevel { get; set; }
    }

    public class EFWeaponModel
    {
        [JsonPropertyName("templateId")]
        public int TemplateId { get; set; }

        [JsonPropertyName("exp")]
        public int Exp { get; set; }

        [JsonPropertyName("weaponLv")]
        public int WeaponLv { get; set; }

        [JsonPropertyName("refineLv")]
        public int RefineLv { get; set; }

        [JsonPropertyName("breakthroughLv")]
        public int BreakthroughLv { get; set; }

        [JsonPropertyName("attachedGem")]
        public EFGemModel AttachedGem { get; set; }
    }

    public class EFGemModel
    {
        [JsonPropertyName("templateId")]
        public int TemplateId { get; set; }

        [JsonPropertyName("totalCost")]
        public int TotalCost { get; set; }

        [JsonPropertyName("terms")]
        public List<EFGemTermModel> Terms { get; set; }

        [JsonPropertyName("domainId")]
        public int DomainId { get; set; }
    }

    public class EFGemTermModel
    {
        [JsonPropertyName("termNumId")]
        public int TermNumId { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }
    }

    public class EFSkillInfoModel
    {
        [JsonPropertyName("levelInfo")]
        public List<EFSkillLevelModel> LevelInfo { get; set; }

        [JsonPropertyName("normalSkill")]
        public string NormalSkill { get; set; }

        [JsonPropertyName("ultimateSkill")]
        public string UltimateSkill { get; set; }

        [JsonPropertyName("comboSkill")]
        public string ComboSkill { get; set; }

        [JsonPropertyName("dispNormalAttackSkill")]
        public string DispNormalAttackSkill { get; set; }
    }

    public class EFSkillLevelModel
    {
        [JsonPropertyName("skillId")]
        public string SkillId { get; set; }

        [JsonPropertyName("skillLevel")]
        public int SkillLevel { get; set; }

        [JsonPropertyName("skillMaxLevel")]
        public int SkillMaxLevel { get; set; }

        [JsonPropertyName("skillEnhancedLevel")]
        public int SkillEnhancedLevel { get; set; }
    }

    public class EFTalentModel
    {
        [JsonPropertyName("latestBreakNode")]
        public string LatestBreakNode { get; set; }

        [JsonPropertyName("attrNodes")]
        public List<string> AttrNodes { get; set; }

        [JsonPropertyName("latestPassiveSkillNodes")]
        public List<string> LatestPassiveSkillNodes { get; set; }

        [JsonPropertyName("latestFactorySkillNodes")]
        public List<string> LatestFactorySkillNodes { get; set; }
    }

    public class EFIntKeyValueModel
    {
        [JsonPropertyName("key")]
        public int Key { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}
