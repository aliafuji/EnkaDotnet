using System.Text.Json;
using EnkaDotNet.Assets.HSR.Models;
using EnkaDotNet.Enums;
using EnkaDotNet.Utils;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Utils.HSR;
using EnkaDotNet.Components.Genshin;

namespace EnkaDotNet.Assets.HSR
{
    public class HSRAssets : BaseAssets, IHSRAssets
    {
        private static readonly Dictionary<string, string> HSRAssetUrls = new()
        {
            { "text_map.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/hsr.json" },
            { "characters.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_characters.json" },
            { "lightcones.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_weps.json" },
            { "relics.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_relics.json" },
            { "avatars.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_avatars.json" },
            { "skills.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_skills.json" },
            { "ranks.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_ranks.json" },
            { "skill_tree.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_skilltree.json" },
            { "meta.json", "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/hsr/honker_meta.json" }
        };

        private readonly Dictionary<string, HSRCharacterAssetInfo> _characters = new();
        private readonly Dictionary<string, HSRLightConeAssetInfo> _lightCones = new();
        private readonly Dictionary<string, HSRPfpAssetInfo> _pfps = new();
        private readonly Dictionary<string, HSRNameCardAssetInfo> _namecards = new();
        private readonly Dictionary<string, HSRPropertyAssetInfo> _properties = new();
        private readonly Dictionary<string, HSRRelicItemInfo> _relicItems = new();
        private readonly Dictionary<string, HSRRelicSetInfo> _relicSets = new();
        private readonly Dictionary<string, HSRSkillAssetInfo> _skills = new();
        private readonly Dictionary<string, HSREidolonAssetInfo> _eidolons = new();
        private Dictionary<string, string>? _localization;
        private HSRMetaData? _metaData;

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
                LoadMetaData()
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
                    _localization = localizationData.FirstOrDefault().Value;
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
                    var avatarInfo = kvp.Value;
                    if (avatarInfo.Icon != null)
                    {
                        _pfps[kvp.Key] = avatarInfo;
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
                var relicItemsMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRRelicItemInfo>>("relics.json");
                foreach (var kvp in relicItemsMap)
                {
                    _relicItems[kvp.Key] = kvp.Value;

                    string setId = kvp.Value.SetID.ToString();
                    if (!_relicSets.ContainsKey(setId))
                    {
                        _relicSets[setId] = new HSRRelicSetInfo
                        {
                            SetName = kvp.Value.SetID.ToString()
                        };
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
                foreach (var kvp in deserializedMap)
                {
                    _skills[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load essential skills data", ex);
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
            if (_eidolons.TryGetValue(eidolonId, out var eidolonInfo))
            {
                return eidolonInfo;
            }
            return null;
        }

        public string GetSkillTreeIconUrl(int skillId)
        {
            string skillIdStr = skillId.ToString();
            if (_skills.TryGetValue(skillIdStr, out var skillInfo) && !string.IsNullOrEmpty(skillInfo.IconPath))
            {
                return $"{Constants.GetAssetBaseUrl(GameType)}{skillInfo.IconPath}";
            }
            return string.Empty;
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
            if (_characters.TryGetValue(characterId, out var characterInfo))
            {
                return characterInfo;
            }
            return null;
        }

        public HSRLightConeAssetInfo? GetLightConeInfo(string lightConeId)
        {
            if (_lightCones.TryGetValue(lightConeId, out var lightConeInfo))
            {
                return lightConeInfo;
            }
            return null;
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
            if (_characters.TryGetValue(characterIdStr, out var characterInfo))
            {
                return characterInfo.Rarity;
            }
            return 0;
        }

        public HSRRelicSetInfo? GetRelicSetInfo(string setId)
        {
            if (_relicSets.TryGetValue(setId, out var setInfo))
            {
                return setInfo;
            }
            return null;
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
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo))
            {
                return lightConeInfo.Rarity;
            }
            return 0;
        }

        public string GetRelicSetName(int setId)
        {
            string setIdStr = setId.ToString();
            if (_relicSets.TryGetValue(setIdStr, out var setInfo) && !string.IsNullOrEmpty(setInfo.SetName))
            {
                string localizedName = GetLocalizedText(setInfo.SetName);
                if (localizedName != setInfo.SetName)
                {
                    return localizedName;
                }
                return setInfo.SetName;
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
            if (_relicItems.TryGetValue(relicIdStr, out var relicInfo))
            {
                return relicInfo.Rarity;
            }
            return 0;
        }

        public int GetRelicSetId(int relicId)
        {
            string relicIdStr = relicId.ToString();
            if (_relicItems.TryGetValue(relicIdStr, out var relicInfo))
            {
                return relicInfo.SetID;
            }
            return 0;
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
            if (Enum.IsDefined(typeof(StatPropertyType), propertyId))
            {
                return ((StatPropertyType)propertyId).ToString();
            }
            return $"Property_{propertyId}";
        }

        public string FormatPropertyValue(int propertyId, double value)
        {
            if (Enum.IsDefined(typeof(StatPropertyType), propertyId))
            {
                var propType = (StatPropertyType)propertyId;
                bool isPercentage = IsPercentageStat(propType);

                if (isPercentage)
                {
                    return $"{value:F1}%";
                }
                else if (propType == StatPropertyType.SpeedDelta)
                {
                    return $"{value:F1}";
                }
                else
                {
                    return $"{(int)value}";
                }
            }
            return value.ToString();
        }

        public string GetNameCardIconUrl(int nameCardId)
        {
            return $"NameCard_{nameCardId}";
        }

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
            if (_metaData?.AvatarStats == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string charId = characterId.ToString();
            string promotion = promotionLevel.ToString();

            if (_metaData.AvatarStats.TryGetValue(charId, out var characterData) &&
                characterData.TryGetValue(promotion, out var stats))
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
            if (_metaData?.EquipmentStats == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string lcId = lightConeId.ToString();
            string promotion = promotionLevel.ToString();

            if (_metaData.EquipmentStats.TryGetValue(lcId, out var lcData) &&
                lcData.TryGetValue(promotion, out var stats))
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
            if (_metaData?.EquipmentSkills == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string lcId = lightConeId.ToString();
            string rankStr = rank.ToString();

            if (_metaData.EquipmentSkills.TryGetValue(lcId, out var lcSkillData) &&
                lcSkillData.TryGetValue(rankStr, out var skillInfo) &&
                skillInfo.Props != null)
            {
                foreach (var prop in skillInfo.Props)
                {
                    result[prop.Key] = prop.Value;
                }
            }

            return result;
        }

        public Dictionary<string, double> GetRelicMainStatInfo(int mainAffixGroup, int mainAffixId, int level)
        {
            if (_metaData?.RelicInfo?.MainAffix == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string groupId = mainAffixGroup.ToString();
            string affixId = mainAffixId.ToString();

            if (_metaData.RelicInfo.MainAffix.TryGetValue(groupId, out var groupData) &&
                groupData.TryGetValue(affixId, out var affixInfo))
            {
                double baseValue = affixInfo.BaseValue;
                double levelAdd = affixInfo.LevelAdd;
                string property = affixInfo.Property ?? "";

                result["BaseValue"] = baseValue;
                result["LevelAdd"] = levelAdd;
                result["Value"] = baseValue + (levelAdd * level);

                if (!string.IsNullOrEmpty(property))
                {
                    result["Property"] = 1;
                    result["PropertyName"] = property.GetHashCode();
                }
            }

            return result;
        }

        public Dictionary<string, double> GetTraceEffects(int traceId, int level)
        {
            if (_metaData?.SkillTreeInfo == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string trace = traceId.ToString();
            string levelStr = level.ToString();

            if (_metaData.SkillTreeInfo.TryGetValue(trace, out var traceData) &&
                traceData.TryGetValue(levelStr, out var levelInfo) &&
                levelInfo.Props != null)
            {
                foreach (var prop in levelInfo.Props)
                {
                    result[prop.Key] = prop.Value;
                }
            }

            return result;
        }

        public Dictionary<string, double> GetRelicSetEffects(int setId, int pieceCount)
        {
            if (_metaData?.RelicInfo?.SetSkill == null) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();
            string setIdStr = setId.ToString();
            string pieceCountStr = pieceCount.ToString();

            if (_metaData.RelicInfo.SetSkill.TryGetValue(setIdStr, out var setData) &&
                setData.TryGetValue(pieceCountStr, out var skillInfo) &&
                skillInfo.Props != null)
            {
                foreach (var prop in skillInfo.Props)
                {
                    result[prop.Key] = prop.Value;
                }
            }

            return result;
        }

        public HSRAvatarMetaStats? GetAvatarStats(string avatarId, int promotion)
        {
            if (_metaData?.AvatarStats == null) return null;

            if (_metaData.AvatarStats.TryGetValue(avatarId, out var promotionMap))
            {
                if (promotionMap.TryGetValue(promotion.ToString(), out var stats))
                {
                    return stats;
                }
            }

            return null;
        }

        public HSREquipmentMetaStats? GetEquipmentStats(string equipmentId, int promotion)
        {
            if (_metaData?.EquipmentStats == null) return null;

            if (_metaData.EquipmentStats.TryGetValue(equipmentId, out var promotionMap))
            {
                if (promotionMap.TryGetValue(promotion.ToString(), out var stats))
                {
                    return stats;
                }
            }

            return null;
        }

        public Dictionary<string, double>? GetEquipmentSkillProps(string skillId, int rank)
        {
            if (_metaData?.EquipmentSkills == null) return null;

            if (_metaData.EquipmentSkills.TryGetValue(skillId, out var rankMap))
            {
                if (rankMap.TryGetValue(rank.ToString(), out var skillInfo))
                {
                    return skillInfo.Props;
                }
            }

            return null;
        }

        public HSRRelicMainAffixInfo? GetRelicMainAffixInfo(int groupId, int affixId)
        {
            if (_metaData?.RelicInfo?.MainAffix == null) return null;

            if (_metaData.RelicInfo.MainAffix.TryGetValue(groupId.ToString(), out var affixMap) &&
                affixMap.TryGetValue(affixId.ToString(), out var affixInfo))
            {
                return affixInfo;
            }

            return null;
        }

        public HSRRelicSubAffixInfo? GetRelicSubAffixInfo(int groupId, int affixId)
        {
            if (_metaData?.RelicInfo?.SubAffix == null) return null;

            if (_metaData.RelicInfo.SubAffix.TryGetValue(groupId.ToString(), out var affixMap) &&
                affixMap.TryGetValue(affixId.ToString(), out var affixInfo))
            {
                return affixInfo;
            }

            return null;
        }

        public Dictionary<string, double> GetRelicMainAffixValueAtLevel(int groupId, int affixId, int level)
        {
            var result = new Dictionary<string, double>();
            var affixInfo = GetRelicMainAffixInfo(groupId, affixId);

            if (affixInfo != null)
            {
                double calculatedValue = affixInfo.BaseValue + (affixInfo.LevelAdd * level);

                result["Value"] = calculatedValue;

                if (!string.IsNullOrEmpty(affixInfo.Property))
                {
                    result["PropertyType"] = (double)HSRStatPropertyUtils.MapToStatPropertyType(affixInfo.Property).GetHashCode();
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
                double calculatedValue = affixInfo.BaseValue + (affixInfo.StepValue * step);

                result["Value"] = calculatedValue;

                if (!string.IsNullOrEmpty(affixInfo.Property))
                {
                    result["PropertyType"] = (double)HSRStatPropertyUtils.MapToStatPropertyType(affixInfo.Property).GetHashCode();
                }
            }

            return result;
        }

        public Dictionary<string, double>? GetSkillTreeProps(string pointId, int level)
        {
            if (_metaData?.SkillTreeInfo == null) return null;

            if (_metaData.SkillTreeInfo.TryGetValue(pointId, out var levelMap))
            {
                if (levelMap.TryGetValue(level.ToString(), out var treeInfo))
                {
                    return treeInfo.Props;
                }
            }

            return null;
        }

        private ElementType MapElementNameToEnum(string elementName)
        {
            return elementName?.ToUpperInvariant() switch
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
        }

        private PathType MapPathNameToEnum(string pathName)
        {
            return pathName?.ToUpperInvariant() switch
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
        }

        private RelicType MapRelicTypeToEnum(string relicType)
        {
            return relicType?.ToUpperInvariant() switch
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

        private bool IsPercentageStat(StatPropertyType statType)
        {
            switch (statType)
            {
                case StatPropertyType.HPAddedRatio:
                case StatPropertyType.AttackAddedRatio:
                case StatPropertyType.DefenceAddedRatio:
                case StatPropertyType.CriticalChance:
                case StatPropertyType.CriticalDamage:
                case StatPropertyType.StatusProbability:
                case StatPropertyType.StatusResistance:
                case StatPropertyType.BreakDamageAddedRatio:
                case StatPropertyType.PhysicalAddedRatio:
                case StatPropertyType.FireAddedRatio:
                case StatPropertyType.IceAddedRatio:
                case StatPropertyType.LightningAddedRatio:
                case StatPropertyType.WindAddedRatio:
                case StatPropertyType.QuantumAddedRatio:
                case StatPropertyType.ImaginaryAddedRatio:
                case StatPropertyType.HealRatioBase:
                case StatPropertyType.SPRatioBase:
                case StatPropertyType.CriticalChanceBase:
                case StatPropertyType.CriticalDamageBase:
                case StatPropertyType.BreakDamageAddedRatioBase:
                    return true;
                default:
                    return false;
            }
        }
    }
}