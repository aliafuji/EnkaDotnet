﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Assets.ZZZ.Models;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    /// <summary>
    /// Calculates character, weapon, and drive disc stats for Zenless Zone Zero
    /// </summary>
    public class ZZZStatsCalculator
    {
        private readonly IZZZAssets _assets;
        private readonly Dictionary<string, object> _calculationCache = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ZZZStatsCalculator"/> class
        /// </summary>
        /// <param name="assets">The ZZZ assets provider</param>
        public ZZZStatsCalculator(IZZZAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        /// <summary>
        /// Calculates the base stats for an agent
        /// </summary>
        public Dictionary<StatType, double> CalculateAgentBaseStats(int agentId, int level, int promotionLevel, int coreSkillEnhancement)
        {
            string cacheKey = $"agent_{agentId}_{level}_{promotionLevel}_{coreSkillEnhancement}";
            if (_calculationCache.TryGetValue(cacheKey, out object cachedStats) && cachedStats is Dictionary<StatType, double> dictStats)
            {
                return dictStats;
            }

            var stats = new Dictionary<StatType, double>();
            var avatarInfo = _assets.GetAvatarInfo(agentId.ToString());
            if (avatarInfo == null || avatarInfo.BaseProps == null) return stats;

            foreach (var prop in avatarInfo.BaseProps)
            {
                if (int.TryParse(prop.Key, out int propertyId) && Enum.IsDefined(typeof(StatType), propertyId))
                {
                    StatType statType = (StatType)propertyId;
                    double baseValueRaw = prop.Value;
                    double growthValueRaw = 0;
                    double promotionValueRaw = 0;
                    double coreEnhancementValueRaw = 0;

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
                    if (avatarInfo.CoreEnhancementProps != null && coreSkillEnhancement >= 0 &&
                        coreSkillEnhancement < avatarInfo.CoreEnhancementProps.Count)
                    {
                        var coreProps = avatarInfo.CoreEnhancementProps[coreSkillEnhancement];
                        if (coreProps.TryGetValue(prop.Key, out int coreValueInt))
                        {
                            coreEnhancementValueRaw = coreValueInt;
                        }
                    }
                    double totalRawContribution = baseValueRaw + Math.Floor(growthValueRaw) + promotionValueRaw + coreEnhancementValueRaw;
                    stats[statType] = totalRawContribution;
                }
            }
            _calculationCache[cacheKey] = stats;
            return stats;
        }

        /// <summary>
        /// Calculates the main and secondary stats for a W-Engine (weapon)
        /// </summary>
        public (ZZZStat MainStat, ZZZStat SecondaryStat) CalculateWeaponStats(int weaponId, int level, int breakLevel)
        {
            string cacheKey = $"weapon_{weaponId}_{level}_{breakLevel}";
            if (_calculationCache.TryGetValue(cacheKey, out object cachedResult) && cachedResult is ValueTuple<ZZZStat, ZZZStat> cachedTuple)
            {
                return cachedTuple;
            }

            var weaponInfo = _assets.GetWeaponInfo(weaponId.ToString());
            var weaponLevelDataList = _assets.GetWeaponLevelData();
            var weaponStarDataList = _assets.GetWeaponStarData();

            if (weaponInfo == null || weaponInfo.MainStat == null || weaponInfo.SecondaryStat == null ||
                weaponLevelDataList == null || weaponStarDataList == null)
            {
                return (new ZZZStat { Type = StatType.None }, new ZZZStat { Type = StatType.None });
            }

            int rarity = weaponInfo.Rarity;
            var levelData = weaponLevelDataList.FirstOrDefault(d => d.Level == level && d.Rarity == rarity);
            if (levelData == null)
            {
                return (new ZZZStat { Type = StatType.None }, new ZZZStat { Type = StatType.None });
            }
            double enhanceRate = levelData.EnhanceRate;

            var starData = weaponStarDataList.FirstOrDefault(d => d.Rarity == rarity && d.BreakLevel == breakLevel);
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

            var result = (mainStat, secondaryStat);
            _calculationCache[cacheKey] = result;
            return result;
        }

        /// <summary>
        /// Calculates the main stat for a Drive Disc
        /// </summary>
        public ZZZStat CalculateDriveDiscMainStat(int propertyId, double baseValue, int discLevel, int propertyLevel, Rarity rarity)
        {
            string cacheKey = $"disc_main_{propertyId}_{baseValue}_{discLevel}_{propertyLevel}_{rarity}";
            if (_calculationCache.TryGetValue(cacheKey, out object cachedStat) && cachedStat is ZZZStat stat)
            {
                return stat;
            }

            var equipmentLevelDataList = _assets.GetEquipmentLevelData();
            if (equipmentLevelDataList == null)
            {
                return CreateStatWithProperScaling(propertyId, baseValue, propertyLevel);
            }

            var discLevelData = equipmentLevelDataList.FirstOrDefault(d => d.Level == discLevel && d.Rarity == (int)rarity);
            if (discLevelData == null)
            {
                return CreateStatWithProperScaling(propertyId, baseValue, propertyLevel);
            }

            double enhanceRate = discLevelData.EnhanceRate;
            double calculatedValue = baseValue * (1 + enhanceRate / 10000.0);
            var resultStat = CreateStatWithProperScaling(propertyId, Math.Floor(calculatedValue), propertyLevel);
            _calculationCache[cacheKey] = resultStat;
            return resultStat;
        }

        /// <summary>
        /// Creates a ZZZStat object with appropriate scaling and percentage flags
        /// </summary>
        public ZZZStat CreateStatWithProperScaling(int propertyId, double rawValue, int level = 0)
        {
            if (!Enum.IsDefined(typeof(StatType), propertyId))
            {
                return new ZZZStat { Type = StatType.None, Value = rawValue, Level = level };
            }
            StatType statType = (StatType)propertyId;
            double calculationValue = rawValue;
            bool isPercentageDisplay = ZZZStatsHelpers.IsDisplayPercentageStat(statType);
            bool isEnergyRegenType = statType == StatType.EnergyRegenBase || statType == StatType.EnergyRegenPercent || statType == StatType.EnergyRegenFlat;


            if (ZZZStatsHelpers.IsCalculationPercentageStat(statType) &&
                statType != StatType.CritRateBase &&
                statType != StatType.CritDMGBase)
            {
                calculationValue = rawValue / 100.0;
            }

            return new ZZZStat
            {
                Type = statType,
                Value = calculationValue,
                Level = level,
                IsPercentage = isPercentageDisplay,
                IsEnergyRegen = isEnergyRegenType
            };
        }

        /// <summary>
        /// Clears the internal calculation cache
        /// </summary>
        public void ClearCache()
        {
            _calculationCache.Clear();
        }
    }
}
