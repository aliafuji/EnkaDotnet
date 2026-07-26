using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.Common;

namespace EnkaDotNet.Utils.ZZZ
{
    public class ZZZStatsCalculator
    {
        private readonly IZZZAssets _assets;

        // Dictionary<,> and ZZZStat are both mutable, so cached entries are never handed out
        // directly: every accessor below returns a defensive copy.
        private readonly ConcurrentDictionary<(int AgentId, int Level, int PromotionLevel, int CoreSkillEnhancement), Dictionary<StatType, double>> _agentBaseStatsCache =
            new ConcurrentDictionary<(int, int, int, int), Dictionary<StatType, double>>();
        private readonly ConcurrentDictionary<(int WeaponId, int Level, int BreakLevel), (ZZZStat MainStat, ZZZStat SecondaryStat)> _weaponStatsCache =
            new ConcurrentDictionary<(int, int, int), (ZZZStat, ZZZStat)>();
        private readonly ConcurrentDictionary<(int PropertyId, double BaseValue, int DiscLevel, int PropertyLevel, Rarity Rarity), ZZZStat> _discMainStatCache =
            new ConcurrentDictionary<(int, double, int, int, Rarity), ZZZStat>();

        public ZZZStatsCalculator(IZZZAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        private static ZZZStat CopyStat(ZZZStat stat)
        {
            return new ZZZStat
            {
                Type = stat.Type,
                Value = stat.Value,
                Level = stat.Level,
                IsPercentage = stat.IsPercentage,
                IsEnergyRegen = stat.IsEnergyRegen
            };
        }

        public Dictionary<StatType, double> CalculateAgentBaseStats(int agentId, int level, int promotionLevel, int coreSkillEnhancement)
        {
            var cacheKey = (agentId, level, promotionLevel, coreSkillEnhancement);
            if (_agentBaseStatsCache.TryGetValue(cacheKey, out var cachedStats))
            {
                return new Dictionary<StatType, double>(cachedStats);
            }

            var avatarInfo = _assets.GetAvatarInfo(agentId.ToString(CultureInfo.InvariantCulture));
            if (avatarInfo?.BaseProps == null) return new Dictionary<StatType, double>();

            var stats = new Dictionary<StatType, double>(avatarInfo.BaseProps.Count);
            foreach (var prop in avatarInfo.BaseProps)
            {
                if (int.TryParse(prop.Key, out int propertyId) && EnumHelper.IsDefinedZZZStatType(propertyId))
                {
                    StatType statType = (StatType)propertyId;
                    double baseValueRaw = prop.Value;
                    double growthValueRaw = 0;
                    double promotionValueRaw = 0;

                    if (avatarInfo.GrowthProps != null && avatarInfo.GrowthProps.TryGetValue(prop.Key, out int growthValueInt))
                    {
                        growthValueRaw = (growthValueInt * (level - 1)) / 10000.0;
                    }
                    if (avatarInfo.PromotionProps != null && promotionLevel > 0 && promotionLevel <= avatarInfo.PromotionProps.Count)
                    {
                        var promotionProps = avatarInfo.PromotionProps[promotionLevel - 1];
                        if (promotionProps.TryGetValue(prop.Key, out int promoValueInt))
                        {
                            promotionValueRaw = promoValueInt;
                        }
                    }
                    double totalRawContribution = baseValueRaw + Math.Floor(growthValueRaw) + promotionValueRaw;
                    stats[statType] = totalRawContribution;
                }
            }

            if (avatarInfo.CoreEnhancementProps != null && coreSkillEnhancement >= 0 &&
                coreSkillEnhancement < avatarInfo.CoreEnhancementProps.Count)
            {
                var coreProps = avatarInfo.CoreEnhancementProps[coreSkillEnhancement];
                foreach (var coreProp in coreProps)
                {
                    if (int.TryParse(coreProp.Key, out int corePropertyId) && EnumHelper.IsDefinedZZZStatType(corePropertyId))
                    {
                        StatType coreStatType = (StatType)corePropertyId;
                        double coreValue = coreProp.Value;

                        stats.TryGetValue(coreStatType, out double existingValue);
                        stats[coreStatType] = existingValue + coreValue;
                    }
                }
            }

            _agentBaseStatsCache[cacheKey] = stats;
            return new Dictionary<StatType, double>(stats);
        }

        public (ZZZStat MainStat, ZZZStat SecondaryStat) CalculateWeaponStats(int weaponId, int level, int breakLevel)
        {
            var cacheKey = (weaponId, level, breakLevel);
            if (_weaponStatsCache.TryGetValue(cacheKey, out var cachedResult))
            {
                return (CopyStat(cachedResult.MainStat), CopyStat(cachedResult.SecondaryStat));
            }

            var weaponInfo = _assets.GetWeaponInfo(weaponId.ToString(CultureInfo.InvariantCulture));
            var weaponLevelDataList = _assets.GetWeaponLevelData();
            var weaponStarDataList = _assets.GetWeaponStarData();

            if (weaponInfo == null || weaponInfo.MainStat == null || weaponInfo.SecondaryStat == null ||
                weaponLevelDataList == null || weaponStarDataList == null)
            {
                return (new ZZZStat { Type = StatType.None }, new ZZZStat { Type = StatType.None });
            }

            int rarity = weaponInfo.Rarity;
            ZZZWeaponLevelItem levelData = null;
            foreach (var d in weaponLevelDataList)
            {
                if (d.Level == level && d.Rarity == rarity)
                {
                    levelData = d;
                    break;
                }
            }

            if (levelData == null)
            {
                return (new ZZZStat { Type = StatType.None }, new ZZZStat { Type = StatType.None });
            }
            double enhanceRate = levelData.EnhanceRate;

            ZZZWeaponStarItem starData = null;
            foreach (var d in weaponStarDataList)
            {
                if (d.Rarity == rarity && d.BreakLevel == breakLevel)
                {
                    starData = d;
                    break;
                }
            }

            if (starData == null)
            {
                starData = new ZZZWeaponStarItem { StarRate = 0, RandRate = 0 };
            }
            double starRate = starData.StarRate;
            double randRate = starData.RandRate;

            int mainStatPropId = weaponInfo.MainStat.PropertyId;
            double mainStatBase = weaponInfo.MainStat.PropertyValue;
            double mainStatValue = mainStatBase * (1 + enhanceRate / 10000.0 + starRate / 10000.0);
            var mainStat = CreateStatWithProperScaling(mainStatPropId, Math.Floor(mainStatValue));

            int secondaryStatPropId = weaponInfo.SecondaryStat.PropertyId;
            double secondaryStatBase = weaponInfo.SecondaryStat.PropertyValue;
            double secondaryStatValue = secondaryStatBase * (1 + randRate / 10000.0);
            var secondaryStat = CreateStatWithProperScaling(secondaryStatPropId, Math.Floor(secondaryStatValue));

            _weaponStatsCache[cacheKey] = (mainStat, secondaryStat);
            return (CopyStat(mainStat), CopyStat(secondaryStat));
        }

        public ZZZStat CalculateDriveDiscMainStat(int propertyId, double baseValue, int discLevel, int propertyLevel, Rarity rarity)
        {
            var cacheKey = (propertyId, baseValue, discLevel, propertyLevel, rarity);
            if (_discMainStatCache.TryGetValue(cacheKey, out var cachedStat))
            {
                return CopyStat(cachedStat);
            }

            var equipmentLevelDataList = _assets.GetEquipmentLevelData();
            if (equipmentLevelDataList == null)
            {
                return CreateStatWithProperScaling(propertyId, baseValue, propertyLevel);
            }

            ZZZEquipmentLevelItem discLevelData = null;
            foreach (var d in equipmentLevelDataList)
            {
                if (d.Level == discLevel && d.Rarity == (int)rarity)
                {
                    discLevelData = d;
                    break;
                }
            }

            if (discLevelData == null)
            {
                return CreateStatWithProperScaling(propertyId, baseValue, propertyLevel);
            }

            double enhanceRate = discLevelData.EnhanceRate;
            double calculatedValue = baseValue * (1 + enhanceRate / 10000.0);
            var resultStat = CreateStatWithProperScaling(propertyId, Math.Floor(calculatedValue), propertyLevel);
            _discMainStatCache[cacheKey] = resultStat;
            return CopyStat(resultStat);
        }

        public ZZZStat CreateStatWithProperScaling(int propertyId, double rawValue, int level = 0)
        {
            if (!EnumHelper.IsDefinedZZZStatType(propertyId))
            {
                return new ZZZStat { Type = StatType.None, Value = rawValue, Level = level };
            }
            StatType statType = (StatType)propertyId;
            double calculationValue = rawValue;
            bool isPercentageDisplay = ZZZStatsHelpers.IsDisplayPercentageStat(statType);
            bool isEnergyRegenType = statType == StatType.EnergyRegenBase || statType == StatType.EnergyRegenPercent || statType == StatType.EnergyRegenFlat;

            return new ZZZStat
            {
                Type = statType,
                Value = calculationValue,
                Level = level,
                IsPercentage = isPercentageDisplay,
                IsEnergyRegen = isEnergyRegenType
            };
        }

        public void ClearCache()
        {
            _agentBaseStatsCache.Clear();
            _weaponStatsCache.Clear();
            _discMainStatCache.Clear();
        }
    }
}