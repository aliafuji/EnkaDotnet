using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Assets.HSR.Models;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Utils;
using EnkaDotNet.Utils.HSR;
using Microsoft.Extensions.Logging;

namespace EnkaDotNet.Assets.HSR
{
    public class HSRAssets : BaseAssets, IHSRAssets
    {
        private readonly ConcurrentDictionary<string, HSRCharacterAssetInfo> _characters = new ConcurrentDictionary<string, HSRCharacterAssetInfo>();
        private readonly ConcurrentDictionary<string, HSRLightConeAssetInfo> _lightCones = new ConcurrentDictionary<string, HSRLightConeAssetInfo>();
        private readonly ConcurrentDictionary<string, HSRPfpAssetInfo> _pfps = new ConcurrentDictionary<string, HSRPfpAssetInfo>();
        private readonly ConcurrentDictionary<string, HSRRelicItemInfo> _relicItems = new ConcurrentDictionary<string, HSRRelicItemInfo>();
        private readonly ConcurrentDictionary<string, HSRRelicSetInfo> _relicSets = new ConcurrentDictionary<string, HSRRelicSetInfo>();
        private readonly ConcurrentDictionary<string, HSRSkillAssetInfo> _skills = new ConcurrentDictionary<string, HSRSkillAssetInfo>();
        private readonly ConcurrentDictionary<string, HSREidolonAssetInfo> _eidolons = new ConcurrentDictionary<string, HSREidolonAssetInfo>();

        private HSRMetaData _metaData;
        private ConcurrentDictionary<string, HSRSkillTreePointInfo> _skillTreeData;
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(3, 3);

        private async Task LoadWithSemaphore(Func<Task> loadFunction)
        {
            await _loadingSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await loadFunction().ConfigureAwait(false);
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        public HSRAssets(string language, HttpClient httpClient, ILogger<HSRAssets> logger)
            : base(language, "hsr", httpClient, logger)
        {
        }

        protected override IReadOnlyDictionary<string, string> GetAssetFileUrls()
        {
            return Constants.HSRAssetFileUrls;
        }

        protected override async Task LoadAssetsInternalAsync()
        {
            var tasks = new List<Task>
            {
                LoadWithSemaphore(LoadMetaData),
                LoadWithSemaphore(LoadCharacters),
                LoadWithSemaphore(LoadLightCones),
                LoadWithSemaphore(LoadRelics),
                LoadWithSemaphore(LoadSkills),
                LoadWithSemaphore(LoadAvatars),
                LoadWithSemaphore(LoadEidolons),
                LoadWithSemaphore(LoadSkillTree)
            };
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task LoadMetaData()
        {
            try
            {
                _metaData = await FetchAndDeserializeAssetAsync<HSRMetaData>("meta.json").ConfigureAwait(false);
                if (_metaData == null)
                {
                    throw new InvalidOperationException("Failed to load essential Honkai: Star Rail metajson data (result was null)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail metajson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail metajson data", ex);
            }
        }

        private async Task LoadEidolons()
        {
            _eidolons.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSREidolonAssetInfo>>("ranks.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _eidolons[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail ranksjson (eidolons) asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail eidolon data", ex);
            }
        }

        private async Task LoadAvatars()
        {
            _pfps.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRPfpAssetInfo>>("avatars.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap)
                    {
                        if (kvp.Value?.Icon != null) _pfps[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail avatarsjson (profile pictures) asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail profile picture data", ex);
            }
        }

        private async Task LoadCharacters()
        {
            _characters.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRCharacterAssetInfo>>("characters.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _characters[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail charactersjson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail character data", ex);
            }
        }

        private async Task LoadLightCones()
        {
            _lightCones.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRLightConeAssetInfo>>("lightcones.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _lightCones[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail lightconesjson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail light cone data", ex);
            }
        }

        private async Task LoadRelics()
        {
            _relicItems.Clear();
            _relicSets.Clear();
            try
            {
                var relicItemsMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRRelicItemInfo>>("relics.json").ConfigureAwait(false);
                if (relicItemsMap != null)
                {
                    foreach (var kvp in relicItemsMap)
                    {
                        _relicItems[kvp.Key] = kvp.Value;
                        string setId = kvp.Value.SetID.ToString();
                        if (!_relicSets.ContainsKey(setId))
                        {
                            string setName = GetText($"RelicSet_{setId}_Name") ?? GetText(kvp.Value.SetID.ToString()) ?? $"Set {setId}";
                            _relicSets[setId] = new HSRRelicSetInfo { SetName = setName };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail relicsjson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail relic data", ex);
            }
        }

        private async Task LoadSkills()
        {
            _skills.Clear();
            try
            {
                var deserializedMap = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRSkillAssetInfo>>("skills.json").ConfigureAwait(false);
                if (deserializedMap != null)
                {
                    foreach (var kvp in deserializedMap) _skills[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail skillsjson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail skill data", ex);
            }
        }

        private async Task LoadSkillTree()
        {
            try
            {
                var data = await FetchAndDeserializeAssetAsync<Dictionary<string, HSRSkillTreePointInfo>>("skill_tree.json").ConfigureAwait(false);
                _skillTreeData = data != null ? new ConcurrentDictionary<string, HSRSkillTreePointInfo>(data) : new ConcurrentDictionary<string, HSRSkillTreePointInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Honkai: Star Rail skill_treejson asset");
                throw new InvalidOperationException("Failed to load essential Honkai: Star Rail skill tree data", ex);
            }
        }

        public string GetLocalizedText(string key) => GetText(key);
        public string GetEidolonIconUrl(int eidolonId)
        {
            string eidolonIdStr = eidolonId.ToString();
            if (_eidolons.TryGetValue(eidolonIdStr, out var eidolonInfo) && !string.IsNullOrEmpty(eidolonInfo.IconPath))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{eidolonInfo.IconPath}";
            }
            return string.Empty;
        }
        public HSREidolonAssetInfo GetEidolonInfo(string eidolonId)
        {
            _eidolons.TryGetValue(eidolonId, out var info);
            return info;
        }

        public string GetCharacterName(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && characterInfo.AvatarName?.Hash != null)
            {
                string localizedName = GetText(characterInfo.AvatarName.Hash);
                if (localizedName != characterInfo.AvatarName.Hash) return localizedName;
            }
            return $"Character_{characterId}";
        }

        public string GetCharacterIconUrl(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.AvatarCutinFrontImgPath))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{characterInfo.AvatarCutinFrontImgPath}";
            }
            return string.Empty;
        }

        public string GetCharacterAvatarIconUrl(int characterId)
        {
            string characterIdStr = characterId.ToString();
            if (_characters.TryGetValue(characterIdStr, out var characterInfo) && !string.IsNullOrEmpty(characterInfo.AvatarSideIconPath))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{characterInfo.AvatarSideIconPath}";
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
            _characters.TryGetValue(characterIdStr, out var characterInfo);
            return characterInfo?.Rarity ?? 0;
        }
        public HSRCharacterAssetInfo GetCharacterInfo(string characterId)
        {
            _characters.TryGetValue(characterId, out var info);
            return info;
        }
        public HSRLightConeAssetInfo GetLightConeInfo(string lightConeId)
        {
            _lightCones.TryGetValue(lightConeId, out var info);
            return info;
        }
        public HSRRelicSetInfo GetRelicSetInfo(string setId)
        {
            if (_relicSets.TryGetValue(setId, out var info))
            {
                if (info != null && (info.SetName == $"Set {setId}" || string.IsNullOrEmpty(info.SetName)))
                {
                    string localizedSetName = GetText($"RelicSet_{setId}_Name") ?? GetText(setId) ?? $"Set {setId}";
                    info.SetName = localizedSetName;
                }
                return info;
            }
            return null;
        }
        public Dictionary<string, HSRRelicSetInfo> GetAllRelicSets() => new Dictionary<string, HSRRelicSetInfo>(_relicSets);
        public string GetLightConeName(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) && lightConeInfo.EquipmentName?.Hash != null)
            {
                return GetText(lightConeInfo.EquipmentName.Hash);
            }
            return $"LightCone_{lightConeId}";
        }

        public string GetLightConeIconUrl(int lightConeId)
        {
            string lightConeIdStr = lightConeId.ToString();
            if (_lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo) && !string.IsNullOrEmpty(lightConeInfo.ImagePath))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{lightConeInfo.ImagePath}";
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
            _lightCones.TryGetValue(lightConeIdStr, out var lightConeInfo);
            return lightConeInfo?.Rarity ?? 0;
        }

        public string GetRelicSetName(int setId)
        {
            string setIdStr = setId.ToString();
            var setInfo = GetRelicSetInfo(setIdStr);
            return setInfo?.SetName ?? $"RelicSet_{setId}";
        }

        public string GetRelicIconUrl(int relicId)
        {
            string relicIdStr = relicId.ToString();
            if (_relicItems.TryGetValue(relicIdStr, out var relicInfo) && !string.IsNullOrEmpty(relicInfo.Icon))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{relicInfo.Icon}";
            }
            return string.Empty;
        }

        public int GetRelicRarity(int relicId)
        {
            string relicIdStr = relicId.ToString();
            _relicItems.TryGetValue(relicIdStr, out var relicInfo);
            return relicInfo?.Rarity ?? 0;
        }

        public int GetRelicSetId(int relicId)
        {
            string relicIdStr = relicId.ToString();
            _relicItems.TryGetValue(relicIdStr, out var relicInfo);
            return relicInfo?.SetID ?? 0;
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
                if (isPercentage) return $"{value * 100:F1}%";
                if (propType == StatPropertyType.SpeedDelta) return $"{value:F1}";
                return $"{(int)value}";
            }
            return value.ToString();
        }

        public string GetProfilePictureIconUrl(int profilePictureId)
        {
            string profilePictureIdstr = profilePictureId.ToString();
            if (_pfps.TryGetValue(profilePictureIdstr, out var pfpInfo) && !string.IsNullOrEmpty(pfpInfo.Icon))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{pfpInfo.Icon}";
            }
            return string.Empty;
        }

        public string GetSkillIconUrl(int skillId)
        {
            string skillIdStr = skillId.ToString();
            if (_skills.TryGetValue(skillIdStr, out var skillInfo) && !string.IsNullOrEmpty(skillInfo.IconPath))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{skillInfo.IconPath}";
            }
            return string.Empty;
        }

        public HSRAvatarMetaStats GetAvatarStats(string avatarId, int promotion)
        {
            if (_metaData?.AvatarStats != null && _metaData.AvatarStats.TryGetValue(avatarId, out var promoDict))
            {
                if (promoDict != null && promoDict.TryGetValue(promotion.ToString(), out var stats)) return stats;
            }
            return null;
        }

        public HSREquipmentMetaStats GetEquipmentStats(string equipmentId, int promotion)
        {
            if (_metaData?.EquipmentStats != null && _metaData.EquipmentStats.TryGetValue(equipmentId, out var promoDict))
            {
                if (promoDict != null && promoDict.TryGetValue(promotion.ToString(), out var stats)) return stats;
            }
            return null;
        }

        public Dictionary<string, double> GetEquipmentSkillProps(string skillId, int rank)
        {
            if (_metaData?.EquipmentSkills != null && _metaData.EquipmentSkills.TryGetValue(skillId, out var rankDict))
            {
                if (rankDict != null && rankDict.TryGetValue(rank.ToString(), out var skillInfo))
                {
                    return skillInfo?.Props ?? new Dictionary<string, double>();
                }
            }
            return new Dictionary<string, double>();
        }

        public HSRRelicMainAffixInfo GetRelicMainAffixInfo(int groupId, int affixId)
        {
            if (_metaData?.RelicInfo?.MainAffix != null &&
                _metaData.RelicInfo.MainAffix.TryGetValue(groupId.ToString(), out var groupDict))
            {
                if (groupDict != null && groupDict.TryGetValue(affixId.ToString(), out var affixInfo))
                    return affixInfo;
            }
            return null;
        }

        public HSRRelicSubAffixInfo GetRelicSubAffixInfo(int groupId, int affixId)
        {
            if (_metaData?.RelicInfo?.SubAffix != null && _metaData.RelicInfo.SubAffix.TryGetValue(groupId.ToString(), out var groupDict))
            {
                if (groupDict != null && groupDict.TryGetValue(affixId.ToString(), out var affixInfo))
                    return affixInfo;
            }
            return null;
        }

        public Dictionary<string, double> GetSkillTreeProps(string pointId, int level)
        {
            if (_metaData?.SkillTreeInfo != null && _metaData.SkillTreeInfo.TryGetValue(pointId, out var levelDict))
            {
                if (levelDict != null && levelDict.TryGetValue(level.ToString(), out var skillInfo))
                    return skillInfo?.Props ?? new Dictionary<string, double>();
            }
            return new Dictionary<string, double>();
        }

        public HSRSkillTreePointInfo GetSkillTreePointInfo(string pointId)
        {
            if (_skillTreeData != null && _skillTreeData.TryGetValue(pointId, out var info)) return info;
            return null;
        }

        public string GetSkillTreePointName(string pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId);
            if (pointInfo?.SkillIds != null && pointInfo.SkillIds.Any())
            {
                string skillIdStr = pointInfo.SkillIds.First().ToString();
                string skillNameKey = $"SkillName_{skillIdStr}";
                string localizedName = GetText(skillNameKey);
                if (!string.IsNullOrEmpty(localizedName) && localizedName != skillNameKey) return localizedName;

                string pointNameKey = $"PointName_{pointId}";
                localizedName = GetText(pointNameKey);
                if (!string.IsNullOrEmpty(localizedName) && localizedName != pointNameKey) return localizedName;
            }
            return $"Trace_{pointId}";
        }

        public string GetSkillTreePointDescription(string pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId);
            if (pointInfo?.SkillIds != null && pointInfo.SkillIds.Any())
            {
                string skillIdStr = pointInfo.SkillIds.First().ToString();
                string skillDescKey = $"SkillDesc_{skillIdStr}";
                string localizedDesc = GetText(skillDescKey);
                if (!string.IsNullOrEmpty(localizedDesc) && localizedDesc != skillDescKey) return localizedDesc;

                string pointDescKey = $"PointDesc_{pointId}";
                localizedDesc = GetText(pointDescKey);
                if (!string.IsNullOrEmpty(localizedDesc) && localizedDesc != pointDescKey) return localizedDesc;
            }
            return $"Description for Trace_{pointId}";
        }

        public string GetSkillTreeIconUrl(int pointId)
        {
            var pointInfo = GetSkillTreePointInfo(pointId.ToString());
            if (pointInfo != null && !string.IsNullOrEmpty(pointInfo.Icon))
            {
                return $"{Constants.DEFAULT_HSR_ASSET_CDN_URL}{pointInfo.Icon}";
            }
            return string.Empty;
        }

        public Dictionary<string, double> GetRelicSetEffects(int setId, int pieceCount)
        {
            if (_metaData?.RelicInfo?.SetSkill == null) return new Dictionary<string, double>();
            string setIdStr = setId.ToString();
            string pieceCountStr = pieceCount.ToString();

            if (_metaData.RelicInfo.SetSkill.TryGetValue(setIdStr, out var setData) &&
                setData != null &&
                setData.TryGetValue(pieceCountStr, out var skillInfo) &&
                skillInfo?.Props != null)
            {
                return skillInfo.Props;
            }
            return new Dictionary<string, double>();
        }
        private ElementType MapElementNameToEnum(string elementName)
        {
            switch (elementName?.ToUpperInvariant())
            {
                case "PHYSICAL": return ElementType.Physical;
                case "FIRE": return ElementType.Fire;
                case "ICE": return ElementType.Ice;
                case "LIGHTNING": case "THUNDER": return ElementType.Lightning;
                case "WIND": return ElementType.Wind;
                case "QUANTUM": return ElementType.Quantum;
                case "IMAGINARY": return ElementType.Imaginary;
                default: return ElementType.Unknown;
            }
        }
        private PathType MapPathNameToEnum(string pathName)
        {
            switch (pathName?.ToUpperInvariant())
            {
                case "WARRIOR": return PathType.Warrior;
                case "ROGUE": return PathType.Rogue;
                case "MAGE": return PathType.Mage;
                case "SHAMAN": return PathType.Shaman;
                case "WARLOCK": return PathType.Warlock;
                case "KNIGHT": return PathType.Knight;
                case "PRIEST": return PathType.Priest;
                case "MEMORY": return PathType.Memory;
                default: return PathType.Unknown;
            }
        }
        private RelicType MapRelicTypeToEnum(string relicType)
        {
            switch (relicType?.ToUpperInvariant())
            {
                case "HEAD": return RelicType.HEAD;
                case "HAND": return RelicType.HAND;
                case "BODY": return RelicType.BODY;
                case "FOOT": return RelicType.FOOT;
                case "NECK": return RelicType.NECK;
                case "OBJECT": return RelicType.OBJECT;
                default: return RelicType.Unknown;
            }
        }
    }
}