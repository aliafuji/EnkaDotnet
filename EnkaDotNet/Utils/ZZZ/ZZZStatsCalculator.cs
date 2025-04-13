using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public class ZZZStatsCalculator
    {
        private readonly IZZZAssets _assets;
        private Dictionary<string, double> _calculationCache = new Dictionary<string, double>();

        public ZZZStatsCalculator(IZZZAssets assets)
        {
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        public Dictionary<StatType, double> CalculateAgentBaseStats(int agentId, int level, int promotionLevel, int coreSkillEnhancement)
        {
            string cacheKey = $"agent_{agentId}_{level}_{promotionLevel}_{coreSkillEnhancement}";

            var stats = new Dictionary<StatType, double>();
            var avatarInfo = _assets.GetAvatarInfo(agentId.ToString());

            if (avatarInfo == null || avatarInfo.BaseProps == null)
                return stats;

            foreach (var prop in avatarInfo.BaseProps)
            {
                if (int.TryParse(prop.Key, out int propertyId) && Enum.IsDefined(typeof(StatType), propertyId))
                {
                    StatType statType = (StatType)propertyId;

                    string propCacheKey = $"{cacheKey}_{propertyId}";

                    if (_calculationCache.TryGetValue(propCacheKey, out double cachedValue))
                    {
                        stats[statType] = cachedValue;
                        continue;
                    }

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

                    double totalRawContribution = baseValueRaw + growthValueRaw + promotionValueRaw + coreEnhancementValueRaw;

                    _calculationCache[propCacheKey] = totalRawContribution;

                    stats[statType] = totalRawContribution;
                }
            }
            return stats;
        }

        public (ZZZStat MainStat, ZZZStat SecondaryStat) CalculateWeaponStats(int weaponId, int level, int breakLevel)
        {
            string cacheKey = $"weapon_{weaponId}_{level}_{breakLevel}";

            if (_calculationCache.TryGetValue(cacheKey + "_main", out double cachedMainValue) &&
                _calculationCache.TryGetValue(cacheKey + "_secondary", out double cachedSecondaryValue) &&
                _calculationCache.TryGetValue(cacheKey + "_mainId", out double cachedMainId) &&
                _calculationCache.TryGetValue(cacheKey + "_secondaryId", out double cachedSecondaryId))
            {
                var mainStatz = CreateStatWithProperScaling((int)cachedMainId, cachedMainValue);
                var secondaryStatz = CreateStatWithProperScaling((int)cachedSecondaryId, cachedSecondaryValue);
                return (mainStatz, secondaryStatz);
            }

            var weaponInfo = _assets.GetWeaponInfo(weaponId.ToString());

            if (weaponInfo?.MainStat == null || weaponInfo?.SecondaryStat == null)
                return (new ZZZStat { Type = StatType.None }, new ZZZStat { Type = StatType.None });

            int mainStatPropId = weaponInfo.MainStat.PropertyId;
            double mainStatBaseAtL1 = weaponInfo.MainStat.PropertyValue;
            double mainStatValue = mainStatBaseAtL1 * GetWeaponLevelMultiplier(weaponId, level, breakLevel);

            int secondaryStatPropId = weaponInfo.SecondaryStat.PropertyId;
            double secondaryStatBaseAtL1 = weaponInfo.SecondaryStat.PropertyValue;
            double secondaryStatValue = secondaryStatBaseAtL1 * GetWeaponAscensionMultiplier(weaponId, breakLevel);

            _calculationCache[cacheKey + "_main"] = mainStatValue;
            _calculationCache[cacheKey + "_secondary"] = secondaryStatValue;
            _calculationCache[cacheKey + "_mainId"] = mainStatPropId;
            _calculationCache[cacheKey + "_secondaryId"] = secondaryStatPropId;

            var mainStat = CreateStatWithProperScaling(mainStatPropId, mainStatValue);
            var secondaryStat = CreateStatWithProperScaling(secondaryStatPropId, secondaryStatValue);

            return (mainStat, secondaryStat);
        }

        private double GetWeaponLevelMultiplier(int weaponId, int level, int breakLevel)
        {
            string cacheKey = $"weapon_level_mult_{weaponId}_{level}_{breakLevel}";

            if (_calculationCache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            double result = (1.0 + 0.1568166666666667 * level + 0.8922 * breakLevel);
            _calculationCache[cacheKey] = result;
            return result;
        }

        private double GetWeaponAscensionMultiplier(int weaponId, int breakLevel)
        {
            string cacheKey = $"weapon_asc_mult_{weaponId}_{breakLevel}";

            if (_calculationCache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            double result = (1.0 + 0.3 * breakLevel);
            _calculationCache[cacheKey] = result;
            return result;
        }

        public ZZZStat CalculateDriveDiscMainStat(int propertyId, double baseValue, int discLevel, int propertyLevel, Rarity rarity)
        {
            string cacheKey = $"disc_main_{propertyId}_{baseValue}_{discLevel}_{propertyLevel}_{rarity}";

            if (_calculationCache.TryGetValue(cacheKey, out double cachedValue))
                return CreateStatWithProperScaling(propertyId, cachedValue, propertyLevel);

            double rarityScale = GetRarityScale(rarity);
            double calculatedValue = baseValue + (baseValue * discLevel * rarityScale);

            _calculationCache[cacheKey] = calculatedValue;

            return CreateStatWithProperScaling(propertyId, calculatedValue, propertyLevel);
        }

        private double GetRarityScale(Rarity rarity)
        {
            string cacheKey = $"rarity_scale_{rarity}";

            if (_calculationCache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            double scale = rarity switch
            {
                Rarity.S => 0.2,
                Rarity.A => 0.25,
                Rarity.B => 0.3,
                _ => 0.2
            };

            _calculationCache[cacheKey] = scale;
            return scale;
        }

        public ZZZStat CreateStatWithProperScaling(int propertyId, double rawValue, int level = 0)
        {
            if (!Enum.IsDefined(typeof(StatType), propertyId))
            {
                return new ZZZStat { Type = StatType.None, Value = rawValue, Level = level };
            }

            StatType statType = (StatType)propertyId;
            double calculationValue = rawValue;
            bool isPercentage = ZZZStatsHelpers.IsDisplayPercentageStat(statType);

            if (ZZZStatsHelpers.IsCalculationPercentageStat(statType) &&
                statType != StatType.CritRateBase &&
                statType != StatType.CritDMGBase &&
                statType != StatType.EnergyRegenBase)
            {
                calculationValue = rawValue / 100;
            }

            return new ZZZStat
            {
                Type = statType,
                Value = calculationValue,
                FormattedValue = (calculationValue * level),
                Level = level,
                IsPercentage = isPercentage
            };
        }

        public void ClearCache()
        {
            _calculationCache.Clear();
        }
    }
}