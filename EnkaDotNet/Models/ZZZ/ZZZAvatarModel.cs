using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Models.ZZZ
{
    public class ZZZAvatarModel
    {
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("PromotionLevel")]
        public int PromotionLevel { get; set; }

        [JsonProperty("Exp")]
        public int Exp { get; set; }

        [JsonProperty("SkinId")]
        public int SkinId { get; set; }

        [JsonProperty("TalentLevel")]
        public int TalentLevel { get; set; }

        [JsonProperty("CoreSkillEnhancement")]
        public int CoreSkillEnhancement { get; set; }

        [JsonProperty("WeaponUid")]
        public int WeaponUid { get; set; }

        [JsonProperty("ObtainmentTimestamp")]
        public long ObtainmentTimestamp { get; set; }

        [JsonProperty("WeaponEffectState")]
        public int WeaponEffectState { get; set; }

        [JsonProperty("TalentToggleList")]
        public List<bool> TalentToggleList { get; set; }

        [JsonProperty("ClaimedRewardList")]
        public List<int> ClaimedRewardList { get; set; }

        [JsonProperty("IsHidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("Weapon")]
        public ZZZWeaponModel Weapon { get; set; }

        [JsonProperty("SkillLevelList")]
        public List<ZZZSkillLevelModel> SkillLevelList { get; set; }

        [JsonProperty("EquippedList")]
        public List<ZZZEquippedItemModel> EquippedList { get; set; }
    }

    public class ZZZSkillLevelModel
    {
        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("Index")]
        public int Index { get; set; }
    }

    public class ZZZEquippedItemModel
    {
        [JsonProperty("Slot")]
        public int Slot { get; set; }

        [JsonProperty("Equipment")]
        public ZZZEquipmentModel Equipment { get; set; }
    }

    public class ZZZEquipmentModel
    {
        [JsonProperty("RandomPropertyList")]
        public List<ZZZPropertyModel> RandomPropertyList { get; set; }

        [JsonProperty("MainPropertyList")]
        public List<ZZZPropertyModel> MainPropertyList { get; set; }

        [JsonProperty("IsAvailable")]
        public bool IsAvailable { get; set; }

        [JsonProperty("IsLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("IsTrash")]
        public bool IsTrash { get; set; }

        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Uid")]
        public int Uid { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonProperty("Exp")]
        public int Exp { get; set; }
    }

    public class ZZZPropertyModel
    {
        [JsonProperty("PropertyId")]
        public int PropertyId { get; set; }

        [JsonProperty("PropertyLevel")]
        public int PropertyLevel { get; set; }

        [JsonProperty("PropertyValue")]
        public int PropertyValue { get; set; }
    }

    public class ZZZWeaponModel
    {
        [JsonProperty("IsAvailable")]
        public bool IsAvailable { get; set; }

        [JsonProperty("IsLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("Uid")]
        public int Uid { get; set; }

        [JsonProperty("Level")]
        public int Level { get; set; }

        [JsonProperty("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonProperty("Exp")]
        public int Exp { get; set; }

        [JsonProperty("UpgradeLevel")]
        public int UpgradeLevel { get; set; }
    }
}