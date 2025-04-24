using System.Text.Json;
using EnkaDotNet.Assets.HSR.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Utils.HSR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace EnkaDotNet.Assets.HSR
{
    public class HSRAssets : BaseAssets, IHSRAssets
    {
        private static readonly Dictionary<string, string> HSRAssetUrls = new()
        {
            { "text_map.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/hsr.json" },
            { "characters.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_characters.json" },
            { "lightcones.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_weps.json" },
            { "relics.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_relics.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_avatars.json" },
            { "skills.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_skills.json" },
            { "ranks.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_ranks.json" },
            { "skill_tree.json", "https://raw.githubusercontent.com/seriaati/enka-py-assets/main/data/hsr/skill_tree.json" },
            { "meta.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_meta.json" }
        };

        private readonly Dictionary<string, HSRCharacterAssetInfo> _characters = new();
        private readonly Dictionary<string, HSRLightConeAssetInfo> _lightCones = new();
        private readonly Dictionary<string, HSRPfpAssetInfo> _pfps = new();
        private readonly Dictionary<string, HSRRelicItemInfo> _relicItems = new();
        private readonly Dictionary<string, HSRRelicSetInfo> _relicSets = new();
        private readonly Dictionary<string, HSRSkillAssetInfo> _skills = new();
        private readonly Dictionary<string, HSREidolonAssetInfo> _eidolons = new();
        private Dictionary<string, string>? _localization;
        private HSRMetaData? _metaData;
        private Dictionary<string, HSRSkillTreePointInfo>? _skillTreeData;


        public HSRAssets(string language = "en")
            : base(language, GameType.HSR)
        {
        }

        protected override Dictionary<string, string> GetAssetUrls() => HSRAssetUrls;

        protected override async Task LoadAssets()
        {
            var tasks = new List<Task>
            {
                LoadLocalizations(),
                LoadCharacters(),
                LoadLightCones(),
                LoadRelics(),
                LoadSkills(),
                LoadAvatars(),
                LoadEidolons(),
                LoadMetaData(),
                LoadSkillTree()
            };
            await Task.WhenAll(tasks);
        }

        private async Task LoadMetaData()
        {
            try
            {
                _metaData = await FetchAndDeserializeAssetAsync<HSRMetaData>("meta.json");
                if (_metaData == null)
                {
                    _metaData = new HSRMetaData();
                }
            }
            catch (Exception ex)
            {
                _metaData = new HSRMetaData();
                throw new InvalidOperationException($"Failed to load essential meta data", ex);
            }
        }

        private async Task LoadEidolons()
        {
            _eidolons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSREidolonAssetInfo>>("ranks.json");
                foreach (var kvp in deserializedMap)
                {
                    _eidolons[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential eidolons data", ex);
            }
        }
        private async Task LoadLocalizations()
        {
            try
            {
                var localizationData = await FetchAndDeserializeAssetAsync<Dictionary<string, Dictionary<string, string>>>("text_map.json");
                if (localizationData.TryGetValue(Language, out var langMap))
                {
                    _localization = langMap;
                }
                else if (localizationData.TryGetValue("en", out var enMap))
                {
                    _localization = enMap;
                }
                else
                {
                    _localization = localizationData.FirstOrDefault().Value ?? new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                _localization = new Dictionary<string, string>();
                throw new InvalidOperationException($"Failed to load essential localization data", ex);
            }
        }

        private async Task LoadAvatars()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRPfpAssetInfo>>("avatars.json");
                foreach (var kvp in deserializedMap)
                {
                    if (kvp.Value?.Icon != null)
                    {
                        _pfps[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential avatars data", ex);
            }
        }
        private async Task LoadCharacters()
        {
            _characters.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRCharacterAssetInfo>>("characters.json");
                _characters.EnsureCapacity(deserializedMap.Count);
                foreach (var kvp in deserializedMap)
                {
                    _characters[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential characters data", ex);
            }
        }

        private async Task LoadLightCones()
        {
            _lightCones.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRLightConeAssetInfo>>("lightcones.json");
                _lightCones.EnsureCapacity(deserializedMap.Count);
                foreach (var kvp in deserializedMap)
                {
                    _lightCones[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential light cones data", ex);
            }
        }

        private async Task LoadRelics()
        {
            _relicItems.Clear();
            _relicSets.Clear();
            try
            {
                var relicData = await FetchAndDeserializeAssetAsync<HSRRelicData>("relics.json");

                if (relicData?.Items != null)
                {
                    _relicItems.EnsureCapacity(relicData.Items.Count);
                    foreach (var kvp in relicData.Items)
                    {
                        _relicItems[kvp.Key] = kvp.Value;
                    }
                }
                if (relicData?.Sets != null)
                {
                    _relicSets.EnsureCapacity(relicData.Sets.Count);
                    foreach (var kvp in relicData.Sets)
                    {
                        _relicSets[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential relics data", ex);
            }
        }

        private async Task LoadSkills()
        {
            _skills.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRSkillAssetInfo>>("skills.json");
                _skills.EnsureCapacity(deserializedMap.Count);
                foreach (var kvp in deserializedMap)
                {
                    _skills[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Assets] Warning: Error loading skills: {ex.Message}. Skill data might be incomplete.");
            }
        }


        private async Task LoadSkillTree()
        {
            _skillTreeData = new Dictionary<string, HSRSkillTreePointInfo>();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRSkillTreePointInfo>>("skill_tree.json");
                _skillTreeData = deserializedMap ?? new Dictionary<string, HSRSkillTreePointInfo>();
            }
            catch (Exception ex)
            {
                _skillTreeData = new Dictionary<string, HSRSkillTreePointInfo>();
            }
        }

        public string GetEidolonIconUrl(int eidolonId)
        {
            string eidolonIdStr = eidolonId.ToString();
            if (_eidolons.TryGetValue(eidolonIdStr, out var eidolonInfo) && !string.IsNullOrEmpty(eidolonInfo.IconPath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{eidolonInfo.IconPath}";
            }
            return string.Empty;
        }
        public HSREidolonAssetInfo? GetEidolonInfo(string eidolonId)
        {
            return _eidolons.GetValueOrDefault(eidolonId);
        }

        public string GetLocalizedText(string key)
        {
            if (_localization != null && !string.IsNullOrEmpty(key) && _localization.TryGetValue(key, out var text))
            {
                return text;
            }
            return key;
        }

        public HSRCharacterAssetInfo? GetCharacterInfo(string characterId)
        {
            return _characters.GetValueOrDefault(characterId);
        }
        public HSRLightConeAssetInfo? GetLightConeInfo(string lightConeId)
        {
            return _lightCones.GetValueOrDefault(lightConeId);
        }

        public string GetCharacterName(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && characterInfo.AvatarName?.Hash != null)
            {
                string localizedName = GetLocalizedText(characterInfo.AvatarName.Hash);
                if (localizedName != characterInfo.AvatarName.Hash)
                {
                    return localizedName;
                }
            }
            return $"Character_{characterId}";
        }

        public string GetCharacterIconUrl(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.AvatarCutinFrontImgPath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{characterInfo.AvatarCutinFrontImgPath}";
            }
            return string.Empty;
        }

        public string GetCharacterAvatarIconUrl(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.AvatarSideIconPath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{characterInfo.AvatarSideIconPath}";
            }
            return string.Empty;
        }

        public ElementType GetCharacterElement(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.Element))
            {
                return MapElementNameToEnum(characterInfo.Element);
            }
            return ElementType.Unknown;
        }

        public PathType GetCharacterPath(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.AvatarBaseType))
            {
                return MapPathNameToEnum(characterInfo.AvatarBaseType);
            }
            return PathType.Unknown;
        }

        public int GetCharacterRarity(int characterId)
        {
            string characterIdStr = characterId.ToString();
            return _characters.TryGetValue(characterIdStr, out var characterInfo) ? characterInfo.Rarity : 0;
        }

        public HSRRelicSetInfo? GetRelicSetInfo(string setId)
        {
            return _relicSets.GetValueOrDefault(setId);
        }
        public Dictionary<string, HSRRelicSetInfo> GetAllRelicSets()
        {
            return _relicSets;
        }

        public string GetLightConeName(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) && lightConeInfo.EquipmentName?.Hash != null)
            {
                return GetLocalizedText(lightConeInfo.EquipmentName.Hash);
            }
            return $"LightCone_{lightConeId}";
        }

        public string GetLightConeIconUrl(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) && !string.IsNullOrEmpty(lightConeInfo.ImagePath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{lightConeInfo.ImagePath}";
            }
            return string.Empty;
        }

        public PathType GetLightConePath(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) && !string.IsNullOrEmpty(lightConeInfo.AvatarBaseType))
            {
                return MapPathNameToEnum(lightConeInfo.AvatarBaseType);
            }
            return PathType.Unknown;
        }

        public int GetLightConeRarity(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            return _lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) ? lightConeInfo.Rarity : 0;
        }

        public string GetRelicSetName(int setId)
        {
            string setIdStr = setId.ToString();
            if (_relicSets.TryGetValue(setIdStr, out var setInfo) && !string.IsNullOrEmpty(setInfo.SetName))
            {
                string localizedName = GetLocalizedText(setInfo.SetName);
                return localizedName != setInfo.SetName ? localizedName : setInfo.SetName;
            }
            return $"RelicSet_{setId}";
        }
        public string GetRelicIconUrl(int relicId)
        {
            string relicIdStr = relicId.ToString();
            if (_relicItems.TryGetValue(relicIdStr, out var relicInfo) && !string.IsNullOrEmpty(relicInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{relicInfo.Icon}";
            }
            return string.Empty;
        }

        public int GetRelicRarity(int relicId)
        {
            string relicIdStr = relicId.ToString();
            return _relicItems.TryGetValue(relicIdStr, out var relicInfo) ? relicInfo.Rarity : 0;
        }

        public int GetRelicSetId(int relicId)
        {
            string relicIdStr = relicId.ToString();
            return _relicItems.TryGetValue(relicIdStr, out var relicInfo) ? relicInfo.SetID : 0;
        }

        public RelicType GetRelicType(int relicId)
        {
            string relicIdStr = relicId.ToString();
            if (_relicItems.TryGetValue(relicIdStr, out var relicInfo) && !string.IsNullOrEmpty(relicInfo.Type))
            {
                return MapRelicTypeToEnum(relicInfo.Type);
            }
            return RelicType.Unknown;
        }

        public string GetPropertyName(int propertyId)
        {
            return Enum.IsDefined(typeof(StatPropertyType), propertyId)
                ? ((StatPropertyType)propertyId).ToString()
                : $"Property_{propertyId}";
        }

        public string FormatPropertyValue(int propertyId, double value)
        {
            if (Enum.IsDefined(typeof(StatPropertyType), propertyId))
            {
                var propType = (StatPropertyType)propertyId;
                bool isPercentage = HSRStatPropertyUtils.IsPercentageType(propType.ToString());

                if (isPercentage) return $"{value:F1}%";
                if (propType == StatPropertyType.SpeedDelta) return $"{value:F1}";
                return $"{(int)value}";
            }
            return value.ToString();
        }

        public string GetNameCardIconUrl(int nameCardId) => $"NameCard_{nameCardId}";

        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            string profilePictureIdstr = profilePictureId.ToString();
            if (_pfps.TryGetValue(profilePictureIdstr, out var pfpInfo) && !string.IsNullOrEmpty(pfpInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{pfpInfo.Icon}";
            }
            return string.Empty;
        }

        public string GetSkillIconUrl(int characterId, SkillType skillType)
        {
            string skillIdStr = $"{characterId}{(int)skillType:D2}";
            if (_skills.TryGetValue(skillIdStr, out var skillInfo) && !string.IsNullOrEmpty(skillInfo.IconPath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{skillInfo.IconPath}";
            }
            return string.Empty;
        }

        public Dictionary<string, double> GetCharacterBaseStats(int characterId, int promotionLevel)
        {
            var result = new Dictionary<string, double>();
            var stats = GetAvatarStats(characterId.ToString(), promotionLevel);
            if (stats != null)
            {
                result["HPBase"] = stats.HPBase;
                result["HPAdd"] = stats.HPAdd;
                result["AttackBase"] = stats.AttackBase;
                result["AttackAdd"] = stats.AttackAdd;
                result["DefenceBase"] = stats.DefenceBase;
                result["DefenceAdd"] = stats.DefenceAdd;
                result["SpeedBase"] = stats.SpeedBase;
                result["CriticalChance"] = stats.CriticalChance;
                result["CriticalDamage"] = stats.CriticalDamage;
            }
            return result;
        }

        public Dictionary<string, double> GetLightConeBaseStats(int lightConeId, int promotionLevel)
        {
            var result = new Dictionary<string, double>();
            var stats = GetEquipmentStats(lightConeId.ToString(), promotionLevel);
            if (stats != null)
            {
                result["BaseHP"] = stats.BaseHP;
                result["HPAdd"] = stats.HPAdd;
                result["BaseAttack"] = stats.BaseAttack;
                result["AttackAdd"] = stats.AttackAdd;
                result["BaseDefence"] = stats.BaseDefence;
                result["DefenceAdd"] = stats.DefenceAdd;
            }
            return result;
        }

        public Dictionary<string, double> GetLightConeSkillEffects(int lightConeId, int rank)
        {
            return GetEquipmentSkillProps(lightConeId.ToString(), rank) ?? new Dictionary<string, double>();
        }

        public Dictionary<string, double> GetRelicSetEffects(int setId, int pieceCount)
        {
            if (_metaData?.RelicInfo?.SetSkill == null) return new Dictionary<string, double>();

            string setIdStr = setId.ToString();
            string pieceCountStr = pieceCount.ToString();

            if (_metaData.RelicInfo.SetSkill.TryGetValue(setIdStr, out var setData) &&
                setData.TryGetValue(pieceCountStr, out var skillInfo) &&
                skillInfo.Props != null)
            {
                return skillInfo.Props;
            }
            return new Dictionary<string, double>();
        }

        public HSRAvatarMetaStats? GetAvatarStats(string avatarId, int promotion)
        {
            return _metaData?.AvatarStats?.GetValueOrDefault(avatarId)?.GetValueOrDefault(promotion.ToString());
        }

        public HSREquipmentMetaStats? GetEquipmentStats(string equipmentId, int promotion)
        {
            return _metaData?.EquipmentStats?.GetValueOrDefault(equipmentId)?.GetValueOrDefault(promotion.ToString());
        }

        public Dictionary<string, double>? GetEquipmentSkillProps(string skillId, int rank)
        {
            return _metaData?.EquipmentSkills?.GetValueOrDefault(skillId)?.GetValueOrDefault(rank.ToString())?.Props;
        }

        public HSRRelicMainAffixInfo? GetRelicMainAffixInfo(int groupId, int affixId)
        {
            return _metaData?.RelicInfo?.MainAffix?.GetValueOrDefault(groupId.ToString())?.GetValueOrDefault(affixId.ToString());
        }

        public HSRRelicSubAffixInfo? GetRelicSubAffixInfo(int groupId, int affixId)
        {
            return _metaData?.RelicInfo?.SubAffix?.GetValueOrDefault(groupId.ToString())?.GetValueOrDefault(affixId.ToString());
        }

        public Dictionary<string, double> GetRelicMainAffixValueAtLevel(int groupId, int affixId, int level)
        {
            var result = new Dictionary<string, double>();
            var affixInfo = GetRelicMainAffixInfo(groupId, affixId);
            if (affixInfo != null)
            {
                result["Value"] = affixInfo.BaseValue + (affixInfo.LevelAdd * level);
                if (!string.IsNullOrEmpty(affixInfo.Property))
                {
                    result["PropertyType"] = (double)HSRStatPropertyUtils.MapToStatPropertyType(affixInfo.Property);
                }
            }
            return result;
        }

        public Dictionary<string, double> GetRelicSubAffixValueAtStep(int groupId, int affixId, int step)
        {
            var result = new Dictionary<string, double>();
            var affixInfo = GetRelicSubAffixInfo(groupId, affixId);
            if (affixInfo != null)
            {
                result["Value"] = affixInfo.BaseValue + (affixInfo.StepValue * step);
                if (!string.IsNullOrEmpty(affixInfo.Property))
                {
                    result["PropertyType"] = (double)HSRStatPropertyUtils.MapToStatPropertyType(affixInfo.Property);
                }
            }
            return result;
        }

        public Dictionary<string, double>? GetSkillTreeProps(string pointId, int level)
        {
            return _metaData?.SkillTreeInfo?.GetValueOrDefault(pointId)?.GetValueOrDefault(level.ToString())?.Props;
        }

        public HSRSkillTreePointInfo? GetSkillTreePointInfo(string pointId)
        {
            return _skillTreeData?.GetValueOrDefault(pointId);
        }

        public string GetSkillTreePointName(string pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId);
            if (pointInfo?.SkillIds != null && pointInfo.SkillIds.Any())
            {
                string skillIdStr = pointInfo.SkillIds.First().ToString();
                if (_skills.TryGetValue(skillIdStr, out var skillInfo) /* && skillInfo has NameHash */ )
                {
                }
            }
            return $"Trace_{pointId}";
        }

        public string GetSkillTreePointDescription(string pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId);
            return $"Description for Trace_{pointId}";
        }

        // --- Added Missing Method Implementations ---

        public Dictionary<string, double> GetRelicMainStatInfo(int mainAffixGroup, int mainAffixId, int level)
        {
            return GetRelicMainAffixValueAtLevel(mainAffixGroup, mainAffixId, level);
        }

        public string GetSkillTreeIconUrl(int pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId.ToString());
            if (pointInfo != null && !string.IsNullOrEmpty(pointInfo.Icon))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{pointInfo.Icon}";
            }
            return string.Empty;
        }

        public Dictionary<string, double> GetTraceEffects(int traceId, int level)
        {
            return GetSkillTreeProps(traceId.ToString(), level) ?? new Dictionary<string, double>();
        }

        // --- End of Added Methods ---


        private ElementType MapElementNameToEnum(string elementName) => elementName?.ToUpperInvariant() switch
        {
            "PHYSICAL" => ElementType.Physical,
            "FIRE" => ElementType.Fire,
            "ICE" => ElementType.Ice,
            "LIGHTNING" => ElementType.Lightning,
            "THUNDER" => ElementType.Lightning,
            "WIND" => ElementType.Wind,
            "QUANTUM" => ElementType.Quantum,
            "IMAGINARY" => ElementType.Imaginary,
            _ => ElementType.Unknown
        };

        private PathType MapPathNameToEnum(string pathName) => pathName?.ToUpperInvariant() switch
        {
            "WARRIOR" => PathType.Warrior,
            "ROGUE" => PathType.Rogue,
            "MAGE" => PathType.Mage,
            "SHAMAN" => PathType.Shaman,
            "WARLOCK" => PathType.Warlock,
            "KNIGHT" => PathType.Knight,
            "PRIEST" => PathType.Priest,
            "MEMORY" => PathType.Memory,
            _ => PathType.Unknown
        };

        private RelicType MapRelicTypeToEnum(string relicType) => relicType?.ToUpperInvariant() switch
        {
            "HEAD" => RelicType.HEAD,
            "HAND" => RelicType.HAND,
            "BODY" => RelicType.BODY,
            "FOOT" => RelicType.FOOT,
            "NECK" => RelicType.NECK,
            "OBJECT" => RelicType.OBJECT,
            _ => RelicType.Unknown
        };
    }
}
