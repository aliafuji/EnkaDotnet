using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZAvatarModel
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("PromotionLevel")]
        public int PromotionLevel { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }

        [JsonPropertyName("SkinId")]
        public int SkinId { get; set; }

        [JsonPropertyName("TalentLevel")]
        public int TalentLevel { get; set; }

        [JsonPropertyName("CoreSkillEnhancement")]
        public int CoreSkillEnhancement { get; set; }

        [JsonPropertyName("UpgradeId")]
        public int UpgradeId { get; set; }

        [JsonPropertyName("WeaponUid")]
        public int WeaponUid { get; set; }

        [JsonPropertyName("ObtainmentTimestamp")]
        public long ObtainmentTimestamp { get; set; }

        [JsonPropertyName("WeaponEffectState")]
        public int WeaponEffectState { get; set; }

        [JsonPropertyName("TalentToggleList")]
        public List<bool> TalentToggleList { get; set; }

        [JsonPropertyName("ClaimedRewardList")]
        public List<int> ClaimedRewardList { get; set; }

        [JsonPropertyName("IsHidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("Weapon")]
        public ZZZWeaponModel Weapon { get; set; }

        [JsonPropertyName("SkillLevelList")]
        public List<ZZZSkillLevelModel> SkillLevelList { get; set; }

        [JsonPropertyName("EquippedList")]
        public List<ZZZEquippedItemModel> EquippedList { get; set; }
    }

    public class ZZZSkillLevelModel
    {
        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("Index")]
        public int Index { get; set; }
    }

    public class ZZZEquippedItemModel
    {
        [JsonPropertyName("Slot")]
        public int Slot { get; set; }

        [JsonPropertyName("Equipment")]
        public ZZZEquipmentModel Equipment { get; set; }
    }

    public class ZZZEquipmentModel
    {
        [JsonPropertyName("RandomPropertyList")]
        public List<ZZZPropertyModel> RandomPropertyList { get; set; }

        [JsonPropertyName("MainPropertyList")]
        public List<ZZZPropertyModel> MainPropertyList { get; set; }

        [JsonPropertyName("IsAvailable")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("IsLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("IsTrash")]
        public bool IsTrash { get; set; }

        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Uid")]
        public int Uid { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }
    }

    public class ZZZPropertyModel
    {
        [JsonPropertyName("PropertyId")]
        public int PropertyId { get; set; }

        [JsonPropertyName("PropertyLevel")]
        public int PropertyLevel { get; set; }

        [JsonPropertyName("PropertyValue")]
        public int PropertyValue { get; set; }
    }

    public class ZZZWeaponModel
    {
        [JsonPropertyName("IsAvailable")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("IsLocked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Uid")]
        public int Uid { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }

        [JsonPropertyName("UpgradeLevel")]
        public int UpgradeLevel { get; set; }
    }
}