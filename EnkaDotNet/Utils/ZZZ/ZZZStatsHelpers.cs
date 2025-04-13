using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public static class ZZZStatsHelpers
    {
        private const double BASE_CRIT_RATE = 0.05;  // 5%
        private const double BASE_CRIT_DMG = 0.50;   // 50%
        private const double BASE_ENERGY_REGEN = 0;
        private static readonly IZZZAssets assets = new ZZZAssets();

        public static Dictionary<string, double> CalculateAllTotalStats(ZZZAgent agent)
        {
            var breakdown = CalculateTotalBreakdown(agent, assets);

            return breakdown.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.TryGetValue("Final", out var total) ? total : 0
            );
        }

        public static bool IsDisplayPercentageStatForGroup(string statGroup)
        {
            switch (statGroup)
            {
                case "Crit Rate":
                case "Crit DMG":
                case "Pen Ratio":
                case "Physical DMG":
                case "Fire DMG":
                case "Ice DMG":
                case "Electric DMG":
                case "Ether DMG":
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDisplayPercentageStat(StatType statType)
        {
            string category = GetStatCategoryDisplay(statType);
            switch (category)
            {
                case "Crit Rate":
                case "Crit DMG":
                case "Pen Ratio":
                case "Physical DMG":
                case "Fire DMG":
                case "Ice DMG":
                case "Electric DMG":
                case "Ether DMG":
                case "HP%":
                case "ATK%":
                case "Def%":
                case "Impact%":
                    return true;
                case "Energy Regen":
                case "HP":
                case "ATK":
                case "Def":
                case "Impact":
                case "Anomaly Mastery":
                case "Anomaly Proficiency":
                case "PEN":
                default:
                    return false;
            }
        }

        public static bool IsCalculationPercentageStat(StatType statType)
        {
            return statType switch
            {
                StatType.HPPercent => true,
                StatType.ATKPercent => true,
                StatType.DefPercent => true,
                StatType.ImpactPercent => true,
                StatType.EnergyRegenPercent => true,
                StatType.AnomalyMasteryPercent => true,

                StatType.CritRateBase => true,
                StatType.CritRateFlat => true,
                StatType.CritDMGBase => true,
                StatType.CritDMGFlat => true,

                StatType.EnergyRegenBase => true,
                StatType.EnergyRegenFlat => true,

                StatType.PenRatioBase => true,
                StatType.PenRatioFlat => true,

                StatType.PhysicalDMGBonusBase => true,
                StatType.PhysicalDMGBonusFlat => true,
                StatType.FireDMGBonusBase => true,
                StatType.FireDMGBonusFlat => true,
                StatType.IceDMGBonusBase => true,
                StatType.IceDMGBonusFlat => true,
                StatType.ElectricDMGBonusBase => true,
                StatType.ElectricDMGBonusFlat => true,
                StatType.EtherDMGBonusBase => true,
                StatType.EtherDMGBonusFlat => true,

                _ => false
            };
        }

        private static Dictionary<string, Dictionary<string, double>> CalculateTotalBreakdown(ZZZAgent agent, IZZZAssets assets)
        {
            var breakdown = InitializeTotalsDictionary();

            foreach (var stat in agent.Stats)
            {
                AddContribution(breakdown, stat.Key, stat.Value, "Agent");
            }

            if (agent.Weapon != null)
            {
                if (agent.Weapon.MainStat.Type == StatType.ATKBase)
                {
                    breakdown["ATK"]["WeaponBase"] += agent.Weapon.MainStat.Value;
                }

                if (agent.Weapon.SecondaryStat.Type == StatType.CritRateFlat)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    if (value > 1.0)
                        value /= 100.0;
                    breakdown["Crit Rate"]["Weapon_Flat"] += value;
                }
                else if (agent.Weapon.SecondaryStat.Type == StatType.CritDMGFlat)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    if (value > 1.0)
                        value /= 100.0;
                    breakdown["Crit DMG"]["Weapon_Flat"] += value;
                }
                else if (agent.Weapon.SecondaryStat.Type == StatType.EnergyRegenPercent)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    breakdown["Energy Regen"]["Weapon_Percent"] += value;
                }
                else
                {
                    AddContribution(breakdown, agent.Weapon.SecondaryStat.Type, agent.Weapon.SecondaryStat.Value, "Weapon");
                }
            }

            foreach (var disc in agent.EquippedDiscs)
            {
                if (disc.MainStat.Type == StatType.CritRateFlat)
                {
                    double value = disc.MainStat.Value;
                    if (value > 1.0)
                        value /= 100.0;

                    breakdown["Crit Rate"]["Discs_Flat"] += value;
                }
                else if (disc.MainStat.Type == StatType.CritDMGFlat)
                {
                    double value = disc.MainStat.Value;
                    if (value > 1.0)
                        value /= 100.0;

                    breakdown["Crit DMG"]["Discs_Flat"] += value;
                }
                else
                {
                    AddContribution(breakdown, disc.MainStat.Type, disc.MainStat.Value, "Discs");
                }

                foreach (var subStat in disc.SubStats)
                {
                    if (subStat.Type == StatType.CritRateFlat)
                    {
                        double baseValue = subStat.Value;
                        if (baseValue > 1.0)
                            baseValue /= 100.0;

                        double scaledValue = baseValue * subStat.Level;
                        breakdown["Crit Rate"]["Discs_Flat"] += scaledValue;
                    }
                    else if (subStat.Type == StatType.CritDMGFlat)
                    {
                        double baseValue = subStat.Value;
                        if (baseValue > 1.0)
                            baseValue /= 100.0;

                        double scaledValue = baseValue * subStat.Level;
                        breakdown["Crit DMG"]["Discs_Flat"] += scaledValue;
                    }
                    else
                    {
                        double originalValue = subStat.Value;
                        double scaledValue = originalValue * subStat.Level;
                        AddContribution(breakdown, subStat.Type, scaledValue, "Discs");
                    }
                }
            }

            ApplySetBonuses(agent, breakdown, assets);

            CalculateFinalValues(breakdown);

            return breakdown;
        }
        private static Dictionary<string, Dictionary<string, double>> InitializeTotalsDictionary()
        {
            var breakdown = new Dictionary<string, Dictionary<string, double>>();
            var statGroups = new List<string> {
                "HP", "ATK", "Def", "Impact", "Crit Rate", "Crit DMG", "Energy Regen",
                "Anomaly Mastery", "Anomaly Proficiency", "Pen Ratio", "PEN",
                "Physical DMG", "Fire DMG", "Ice DMG", "Electric DMG", "Ether DMG"
            };

            foreach (var group in statGroups)
            {
                breakdown[group] = new Dictionary<string, double> {
                    { "Agent_Base", 0 }, { "Agent_Flat", 0 }, { "Agent_Percent", 0 },
                    { "WeaponBase", 0 },
                    { "Weapon_Flat", 0 }, { "Weapon_Percent", 0 },
                    { "Discs_Flat", 0 }, { "Discs_Percent", 0 },
                    { "SetBonus_Flat", 0 }, { "SetBonus_Percent", 0 },
                    { "Final", 0 }
                };
            }

            breakdown["Crit Rate"]["Agent_Base"] = BASE_CRIT_RATE;
            breakdown["Crit DMG"]["Agent_Base"] = BASE_CRIT_DMG;
            breakdown["Energy Regen"]["Agent_Base"] = BASE_ENERGY_REGEN;

            return breakdown;
        }

        private static void AddContribution(Dictionary<string, Dictionary<string, double>> breakdown, StatType statType, double value, string sourcePrefix)
        {
            string category = GetStatCategory(statType);
            if (string.IsNullOrEmpty(category) || !breakdown.ContainsKey(category)) return;

            string targetBucketSuffix;
            double valueToAdd = value;

            if (category == "Crit Rate" || category == "Crit DMG")
            {
                if (sourcePrefix == "Agent" && statType.ToString().EndsWith("Base"))
                {
                    targetBucketSuffix = "Base";
                    valueToAdd = (category == "Crit Rate") ? BASE_CRIT_RATE : BASE_CRIT_DMG;
                }
                else if (sourcePrefix != "Agent")
                {
                    targetBucketSuffix = "Flat";

                    if (value > 1.0)
                    {
                        valueToAdd = value / 100.0;
                    }
                }
                else
                {
                    targetBucketSuffix = "Flat";
                }
            }
            else
            {
                bool isPercentForCalc = IsCalculationPercentageStat(statType);
                bool isAgentBase = sourcePrefix == "Agent" && statType.ToString().EndsWith("Base");

                if (sourcePrefix == "Agent")
                {
                    if (isAgentBase)
                    {
                        if (statType == StatType.EnergyRegenBase)
                        {
                            valueToAdd = value / 100;
                        }
                        targetBucketSuffix = "Base";
                    }
                    else if (isPercentForCalc)
                    {
                        valueToAdd = value / 100.0;
                        targetBucketSuffix = "Percent";
                    }
                    else
                    {
                        targetBucketSuffix = "Flat";
                    }
                }
                else
                {
                    if (isPercentForCalc)
                    {
                        if (sourcePrefix == "SetBonus")
                        {
                            valueToAdd = value / 100.0;
                        }

                        if (statType == StatType.EnergyRegenFlat || statType == StatType.PenRatioFlat ||
                            statType.ToString().Contains("DMGBonusFlat"))
                        {
                            targetBucketSuffix = "Flat";
                        }
                        else
                        {
                            targetBucketSuffix = "Percent";
                        }
                    }
                    else
                    {
                        targetBucketSuffix = "Flat";
                    }
                }
            }

            string bucketKey = $"{sourcePrefix}_{targetBucketSuffix}";

            if (targetBucketSuffix == "Base" && sourcePrefix != "Agent")
            {
                bucketKey = sourcePrefix + targetBucketSuffix;
            }

            if (!breakdown[category].ContainsKey(bucketKey))
            {
                breakdown[category][bucketKey] = 0;
            }

            breakdown[category][bucketKey] += valueToAdd;
        }

        private static void ApplySetBonuses(ZZZAgent agent, Dictionary<string, Dictionary<string, double>> breakdown, IZZZAssets assets)
        {
            var equippedSets = agent.EquippedDiscs
                .GroupBy(d => d.SuitId)
                .Where(g => g.Count() >= 2)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var set in equippedSets)
            {
                int suitId = set.Key;
                int pieceCount = set.Value;
                var suitInfo = assets.GetDiscSetInfo(suitId.ToString());

                if (suitInfo?.SetBonusProps == null || !suitInfo.SetBonusProps.Any()) continue;

                var applicableBonusProps = suitInfo.SetBonusProps;

                foreach (var prop in applicableBonusProps)
                {
                    if (int.TryParse(prop.Key, out int propertyId) && Enum.IsDefined(typeof(StatType), propertyId))
                    {
                        StatType statType = (StatType)propertyId;
                        double rawValue = prop.Value;
                        string category = GetStatCategory(statType);

                        if (statType == StatType.EnergyRegenPercent)
                        {
                            rawValue = rawValue / 1000.0;
                            breakdown["Energy Regen"]["SetBonus_Percent"] += rawValue;
                        }
                        else if (category == "Crit Rate" || category == "Crit DMG")
                        {
                            rawValue = rawValue / 1000.0;
                            breakdown[category]["SetBonus_Flat"] += rawValue;
                        }
                        else if (IsCalculationPercentageStat(statType))
                        {
                            rawValue = rawValue / 1000.0;
                            breakdown[category]["SetBonus_Percent"] += rawValue;
                        }
                        else
                        {
                            breakdown[category]["SetBonus_Flat"] += rawValue;
                        }
                    }
                }
            }
        }

        private static void CalculateFinalValues(Dictionary<string, Dictionary<string, double>> breakdown)
        {
            foreach (var category in breakdown.Keys)
            {
                var catBreakdown = breakdown[category];

                double totalPercentBonus = (catBreakdown["Agent_Percent"] + catBreakdown["Weapon_Percent"] +
                                 catBreakdown["Discs_Percent"] + catBreakdown["SetBonus_Percent"]) / 100.0;

                double totalFlatBonus = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                      catBreakdown["Discs_Flat"];

                double agentBase = catBreakdown["Agent_Base"];
                double weaponBase = catBreakdown.ContainsKey("WeaponBase") ? catBreakdown["WeaponBase"] : 0;

                double finalValue = 0;

                double bonusFlat = catBreakdown["SetBonus_Flat"] / 10;

                switch (category)
                {
                    case "HP":
                        double flooredHPBase = Math.Floor(agentBase);
                        double percentBonus = flooredHPBase * totalPercentBonus;
                        finalValue = flooredHPBase + Math.Round(percentBonus) + totalFlatBonus;
                        catBreakdown["BaseDisplay"] = flooredHPBase;
                        catBreakdown["AddedDisplay"] = finalValue - flooredHPBase;
                        break;
                    case "ATK":
                        double flooredAgentATK = Math.Floor(agentBase);
                        double flooredWeaponATK = Math.Floor(weaponBase);
                        double combinedBaseATK = flooredAgentATK + flooredWeaponATK;
                        double discPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double weaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double agentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double setBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;
                        double atkDiscPercentBonus = combinedBaseATK * discPercentBonus;
                        double atkSetBonusValue = combinedBaseATK * setBonusPercent;
                        double atkAgentWeaponPercentBonus = combinedBaseATK * (agentPercentBonus + weaponPercentBonus);
                        double flatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                           catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];
                        double addedValue = Math.Floor(atkDiscPercentBonus) + Math.Round(atkSetBonusValue) + Math.Floor(atkAgentWeaponPercentBonus) + flatBonuses;
                        finalValue = combinedBaseATK + addedValue;
                        catBreakdown["BaseDisplay"] = combinedBaseATK;
                        catBreakdown["AddedDisplay"] = addedValue;
                        break;

                    case "Def":
                        double flooredDefBase = Math.Floor(agentBase);

                        double defDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double defWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double defAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double defSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double defDiscValue = flooredDefBase * defDiscPercentBonus;
                        double defSetValue = flooredDefBase * defSetBonusPercent;
                        double defAgentWeaponValue = flooredDefBase * (defAgentPercentBonus + defWeaponPercentBonus);

                        double defFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                              catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double defAddedValue = Math.Floor(defDiscValue) + Math.Round(defSetValue) +
                                             Math.Floor(defAgentWeaponValue) + defFlatBonuses;

                        finalValue = flooredDefBase + defAddedValue;
                        catBreakdown["BaseDisplay"] = flooredDefBase;
                        catBreakdown["AddedDisplay"] = defAddedValue;
                        break;

                    case "Impact":
                        double flooredImpactBase = Math.Floor(agentBase);

                        double impactDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double impactWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double impactAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double impactSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double impactDiscValue = flooredImpactBase * impactDiscPercentBonus;
                        double impactSetValue = flooredImpactBase * impactSetBonusPercent;
                        double impactAgentWeaponValue = flooredImpactBase * (impactAgentPercentBonus + impactWeaponPercentBonus);

                        double impactFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                 catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double impactAddedValue = Math.Floor(impactDiscValue) + Math.Round(impactSetValue) +
                                               Math.Round(impactAgentWeaponValue) + impactFlatBonuses;

                        finalValue = flooredImpactBase + impactAddedValue;
                        catBreakdown["BaseDisplay"] = flooredImpactBase;
                        catBreakdown["AddedDisplay"] = impactAddedValue;
                        break;

                    case "Anomaly Mastery":
                        double flooredAnomalyMasteryBase = Math.Floor(agentBase);

                        double anomalyMasteryDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double anomalyMasteryWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double anomalyMasteryAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double anomalyMasterySetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double anomalyMasteryDiscValue = flooredAnomalyMasteryBase * anomalyMasteryDiscPercentBonus;
                        double anomalyMasterySetValue = flooredAnomalyMasteryBase * anomalyMasterySetBonusPercent;
                        double anomalyMasteryAgentWeaponValue = flooredAnomalyMasteryBase * (anomalyMasteryAgentPercentBonus + anomalyMasteryWeaponPercentBonus);

                        double anomalyMasteryFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                         catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double anomalyMasteryAddedValue = Math.Floor(anomalyMasteryDiscValue) + Math.Round(anomalyMasterySetValue) +
                                                       Math.Floor(anomalyMasteryAgentWeaponValue) + anomalyMasteryFlatBonuses;

                        finalValue = flooredAnomalyMasteryBase + anomalyMasteryAddedValue;
                        catBreakdown["BaseDisplay"] = flooredAnomalyMasteryBase;
                        catBreakdown["AddedDisplay"] = anomalyMasteryAddedValue;
                        break;

                    case "Anomaly Proficiency":
                        double flooredAnomalyProficiencyBase = Math.Floor(agentBase);

                        double anomalyProficiencyDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double anomalyProficiencyWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double anomalyProficiencyAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double anomalyProficiencySetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double anomalyProficiencyDiscValue = flooredAnomalyProficiencyBase * anomalyProficiencyDiscPercentBonus;
                        double anomalyProficiencySetValue = flooredAnomalyProficiencyBase * anomalyProficiencySetBonusPercent;
                        double anomalyProficiencyAgentWeaponValue = flooredAnomalyProficiencyBase * (anomalyProficiencyAgentPercentBonus + anomalyProficiencyWeaponPercentBonus);

                        double anomalyProficiencyFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                             catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double anomalyProficiencyAddedValue = Math.Floor(anomalyProficiencyDiscValue) + Math.Round(anomalyProficiencySetValue) +
                                                           Math.Floor(anomalyProficiencyAgentWeaponValue) + anomalyProficiencyFlatBonuses;

                        finalValue = flooredAnomalyProficiencyBase + anomalyProficiencyAddedValue;
                        catBreakdown["BaseDisplay"] = flooredAnomalyProficiencyBase;
                        catBreakdown["AddedDisplay"] = anomalyProficiencyAddedValue;
                        break;

                    case "PEN":
                        double flooredPENBase = Math.Floor(agentBase);

                        double penDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double penWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double penAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double penSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double penDiscValue = flooredPENBase * penDiscPercentBonus;
                        double penSetValue = flooredPENBase * penSetBonusPercent;
                        double penAgentWeaponValue = flooredPENBase * (penAgentPercentBonus + penWeaponPercentBonus);

                        double penFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                              catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double penAddedValue = Math.Floor(penDiscValue) + Math.Round(penSetValue) +
                                             Math.Floor(penAgentWeaponValue) + penFlatBonuses;

                        finalValue = flooredPENBase + penAddedValue;
                        catBreakdown["BaseDisplay"] = flooredPENBase;
                        catBreakdown["AddedDisplay"] = penAddedValue;
                        break;

                    case "Crit Rate":
                        finalValue = (BASE_CRIT_RATE + totalFlatBonus + totalPercentBonus + bonusFlat) * 100;
                        catBreakdown["BaseDisplay"] = (BASE_CRIT_RATE * 100);
                        catBreakdown["AddedDisplay"] = totalFlatBonus + totalPercentBonus + bonusFlat;
                        break;

                    case "Crit DMG":
                        finalValue = (BASE_CRIT_DMG + totalFlatBonus + totalPercentBonus + bonusFlat) * 100;
                        catBreakdown["BaseDisplay"] = (BASE_CRIT_DMG * 100);
                        catBreakdown["AddedDisplay"] = totalFlatBonus + totalPercentBonus + bonusFlat;
                        break;

                    case "Energy Regen":
                        double weaponRegenBonus = catBreakdown["Weapon_Percent"];
                        double discRegenBonus = catBreakdown["Discs_Percent"];
                        double setBonusRegenBonus = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] - 0.91) : 0;

                        finalValue = agentBase + totalFlatBonus + discRegenBonus + setBonusRegenBonus;
                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = totalFlatBonus + weaponRegenBonus + discRegenBonus + setBonusRegenBonus;
                        break;
                    case "Pen Ratio":
                    case "Physical DMG":
                    case "Fire DMG":
                    case "Ice DMG":
                    case "Electric DMG":
                    case "Ether DMG":
                        double elemDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double elemWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double elemAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double elemSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] * 10.0) : 0;

                        double elemDiscValue = elemDiscPercentBonus;
                        double elemSetValue = elemSetBonusPercent;
                        double elemAgentWeaponValue = elemAgentPercentBonus + elemWeaponPercentBonus;

                        double elemFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                               catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double elemTotalValue = elemDiscValue + elemSetValue + elemAgentWeaponValue + elemFlatBonuses;

                        finalValue = elemTotalValue;
                        catBreakdown["BaseDisplay"] = 0;
                        catBreakdown["AddedDisplay"] = finalValue;
                        break;
                    default:
                        finalValue = agentBase * (1.0 + totalPercentBonus) + totalFlatBonus;
                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;
                }

                catBreakdown["Final"] = finalValue;
            }
        }

        public static string GetStatCategory(StatType statType)
        {
            switch (statType)
            {
                case StatType.HPBase: case StatType.HPPercent: case StatType.HPFlat: return "HP";
                case StatType.ATKBase: case StatType.ATKPercent: case StatType.ATKFlat: return "ATK";
                case StatType.DefBase: case StatType.DefPercent: case StatType.DefFlat: return "Def";
                case StatType.ImpactBase: case StatType.ImpactPercent: return "Impact";

                case StatType.CritRateBase: case StatType.CritRateFlat: return "Crit Rate";
                case StatType.CritDMGBase: case StatType.CritDMGFlat: return "Crit DMG";

                case StatType.EnergyRegenBase: case StatType.EnergyRegenPercent: case StatType.EnergyRegenFlat: return "Energy Regen";

                case StatType.AnomalyMasteryBase: case StatType.AnomalyMasteryPercent: case StatType.AnomalyMasteryFlat: return "Anomaly Mastery";
                case StatType.AnomalyProficiencyBase: case StatType.AnomalyProficiencyFlat: return "Anomaly Proficiency";

                case StatType.PenRatioBase: case StatType.PenRatioFlat: return "Pen Ratio";
                case StatType.PENBase: case StatType.PENFlat: return "PEN";

                case StatType.PhysicalDMGBonusBase: case StatType.PhysicalDMGBonusFlat: return "Physical DMG";
                case StatType.FireDMGBonusBase: case StatType.FireDMGBonusFlat: return "Fire DMG";
                case StatType.IceDMGBonusBase: case StatType.IceDMGBonusFlat: return "Ice DMG";
                case StatType.ElectricDMGBonusBase: case StatType.ElectricDMGBonusFlat: return "Electric DMG";
                case StatType.EtherDMGBonusBase: case StatType.EtherDMGBonusFlat: return "Ether DMG";

                default: return "";
            }
        }

        public static string GetStatCategoryDisplay(StatType statType)
        {
            switch (statType)
            {
                case StatType.HPBase: case StatType.HPFlat: return "HP";
                case StatType.ATKBase: case StatType.ATKFlat: return "ATK";
                case StatType.DefBase: case StatType.DefFlat: return "Def";
                case StatType.ImpactBase: return "Impact";

                case StatType.HPPercent: return "HP%";
                case StatType.ATKPercent: return "ATK%";
                case StatType.DefPercent: return "Def%";
                case StatType.ImpactPercent: return "Impact%";

                case StatType.CritRateBase: case StatType.CritRateFlat: return "Crit Rate";
                case StatType.CritDMGBase: case StatType.CritDMGFlat: return "Crit DMG";

                case StatType.EnergyRegenBase: case StatType.EnergyRegenPercent: case StatType.EnergyRegenFlat: return "Energy Regen";

                case StatType.AnomalyMasteryBase: case StatType.AnomalyMasteryPercent: case StatType.AnomalyMasteryFlat: return "Anomaly Mastery";
                case StatType.AnomalyProficiencyBase: case StatType.AnomalyProficiencyFlat: return "Anomaly Proficiency";

                case StatType.PenRatioBase: case StatType.PenRatioFlat: return "Pen Ratio";
                case StatType.PENBase: case StatType.PENFlat: return "PEN";

                case StatType.PhysicalDMGBonusBase: case StatType.PhysicalDMGBonusFlat: return "Physical DMG";
                case StatType.FireDMGBonusBase: case StatType.FireDMGBonusFlat: return "Fire DMG";
                case StatType.IceDMGBonusBase: case StatType.IceDMGBonusFlat: return "Ice DMG";
                case StatType.ElectricDMGBonusBase: case StatType.ElectricDMGBonusFlat: return "Electric DMG";
                case StatType.EtherDMGBonusBase: case StatType.EtherDMGBonusFlat: return "Ether DMG";

                default: return "";
            }
        }

        public static StatBreakdown GetStatSourceBreakdown(ZZZAgent agent, string statGroup)
        {
            var breakdownData = CalculateTotalBreakdown(agent, assets);
            if (!breakdownData.TryGetValue(statGroup, out var catBreakdown))
            {
                return new StatBreakdown { StatGroup = statGroup };
            }

            var result = new StatBreakdown
            {
                StatGroup = statGroup,
                IsPercentage = IsDisplayPercentageStatForGroup(statGroup)
            };

            result.TotalValue = catBreakdown["Final"];

            if (statGroup == "Crit Rate")
            {
                result.BaseValue = (BASE_CRIT_RATE * 100);
            }
            else if (statGroup == "Crit DMG")
            {
                result.BaseValue = (BASE_CRIT_DMG * 100);
            }
            else
            {
                result.BaseValue = catBreakdown["Agent_Base"];
                if (statGroup == "ATK") result.BaseValue += catBreakdown["WeaponBase"];
                if (statGroup == "Energy Regen") result.BaseValue = catBreakdown["Agent_Base"];
                if (statGroup.Contains("DMG") || statGroup == "Pen Ratio") result.BaseValue = 0;
            }

            result.WeaponValue = catBreakdown["Weapon_Flat"] + catBreakdown["Weapon_Percent"];
            result.DiscsValue = catBreakdown["Discs_Flat"] + catBreakdown["Discs_Percent"];
            result.SetBonusValue = catBreakdown["SetBonus_Flat"] + catBreakdown["SetBonus_Percent"];

            if (result.TotalValue != 0)
            {
                double addedValue = result.TotalValue - result.BaseValue;
                if (addedValue != 0)
                {
                    result.WeaponContributionPercent = result.WeaponValue / addedValue;
                    result.DiscsContributionPercent = result.DiscsValue / addedValue;
                    result.SetBonusContributionPercent = result.SetBonusValue / addedValue;
                }
                result.BaseContributionPercent = result.BaseValue / result.TotalValue;
            }

            return result;
        }
    }

    public class StatBreakdown
    {
        public string StatGroup { get; set; } = string.Empty;
        public bool IsPercentage { get; set; }

        public double BaseValue { get; set; }
        public double WeaponValue { get; set; }
        public double DiscsValue { get; set; }
        public double SetBonusValue { get; set; }
        public double TotalValue { get; set; }

        public double BaseContributionPercent { get; set; }
        public double WeaponContributionPercent { get; set; }
        public double DiscsContributionPercent { get; set; }
        public double SetBonusContributionPercent { get; set; }

        public ZZZStatValue Base => new ZZZStatValue(BaseValue, IsPercentage);
        public ZZZStatValue Weapon => new ZZZStatValue(WeaponValue, IsPercentage);
        public ZZZStatValue Discs => new ZZZStatValue(DiscsValue, IsPercentage);
        public ZZZStatValue SetBonus => new ZZZStatValue(SetBonusValue, IsPercentage);
        public ZZZStatValue Total => new ZZZStatValue(TotalValue, IsPercentage);

        public double AddedValue => TotalValue - BaseValue;
        public ZZZStatValue Added => new ZZZStatValue(AddedValue, IsPercentage);
    }
}