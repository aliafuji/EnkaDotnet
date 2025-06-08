using System;
using System.Collections.Generic;
using System.Linq;
using EnkaDotNet.Assets.ZZZ;
using EnkaDotNet.Components.ZZZ;
using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Utils.ZZZ
{
    public static class ZZZStatsHelpers
    {
        private const double BASE_CRIT_RATE = 0;
        private const double BASE_CRIT_DMG = 0;
        private const double BASE_ENERGY_REGEN = 0;

        public static Dictionary<string, StatSummary> CalculateAllTotalStats(ZZZAgent agent, IZZZAssets assets)
        {
            var breakdown = CalculateTotalBreakdown(agent, assets);
            return breakdown.ToDictionary(
                kvp => kvp.Key,
                kvp => new StatSummary
                {
                    FinalValue = kvp.Value.TryGetValue("Final", out var final) ? final : 0,
                    BaseValue = kvp.Value.TryGetValue("BaseDisplay", out var baseVal) ? baseVal : 0,
                    AddedValue = kvp.Value.TryGetValue("AddedDisplay", out var addedVal) ? addedVal : 0
                }
            );
        }

        public static bool IsDisplayPercentageStatForGroup(string statGroup)
        {
            switch (statGroup)
            {
                case "CRIT Rate":
                case "CRIT DMG":
                case "Pen Ratio":
                case "Physical DMG":
                case "Fire DMG":
                case "Ice DMG":
                case "Electric DMG":
                case "Ether DMG":
                case "Energy Regen":
                    return true;
                default: return false;
            }
        }

        public static bool IsDisplayPercentageStat(StatType statType)
        {
            string category = GetStatCategoryDisplay(statType);
            switch (category)
            {
                case "CRIT Rate":
                case "CRIT DMG":
                case "Pen Ratio":
                case "Physical DMG":
                case "Fire DMG":
                case "Ice DMG":
                case "Electric DMG":
                case "Ether DMG":
                case "HP%":
                case "ATK%":
                case "DEF%":
                case "Impact":
                case "Energy Regen":
                    return true;
                default: return false;
            }
        }

        public static bool IsCalculationPercentageStat(StatType statType)
        {
            switch (statType)
            {
                case StatType.HPPercent:
                case StatType.ATKPercent:
                case StatType.DefPercent:
                case StatType.ImpactPercent:
                case StatType.EnergyRegenPercent:
                case StatType.AnomalyMasteryPercent:
                case StatType.CritRateBase:
                case StatType.CritRateFlat:
                case StatType.CritDMGBase:
                case StatType.CritDMGFlat:
                case StatType.EnergyRegenBase:
                case StatType.EnergyRegenFlat:
                case StatType.PenRatioBase:
                case StatType.PenRatioFlat:
                case StatType.PhysicalDMGBonusBase:
                case StatType.PhysicalDMGBonusFlat:
                case StatType.FireDMGBonusBase:
                case StatType.FireDMGBonusFlat:
                case StatType.IceDMGBonusBase:
                case StatType.IceDMGBonusFlat:
                case StatType.ElectricDMGBonusBase:
                case StatType.ElectricDMGBonusFlat:
                case StatType.EtherDMGBonusBase:
                case StatType.EtherDMGBonusFlat:
                    return true;
                default: return false;
            }
        }

        internal static Dictionary<string, Dictionary<string, double>> CalculateTotalBreakdown(ZZZAgent agent, IZZZAssets assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets), "IZZZAssets instance cannot be null for calculating stats.");
            }

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
                    breakdown["CRIT Rate"]["Weapon_Flat"] += value;
                }
                else if (agent.Weapon.SecondaryStat.Type == StatType.CritDMGFlat)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    if (value > 1.0)
                        value /= 100.0;
                    breakdown["CRIT DMG"]["Weapon_Flat"] += value;
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

                    breakdown["CRIT Rate"]["Discs_Flat"] += value;
                }
                else if (disc.MainStat.Type == StatType.CritDMGFlat)
                {
                    double value = disc.MainStat.Value;
                    if (value > 1.0)
                        value /= 100.0;

                    breakdown["CRIT DMG"]["Discs_Flat"] += value;
                }
                else
                {
                    AddContribution(breakdown, disc.MainStat.Type, disc.MainStat.Value, "Discs");
                }

                foreach (var subStat in disc.SubStatsRaw)
                {
                    if (subStat.Type == StatType.CritRateFlat)
                    {
                        double baseValue = subStat.Value;
                        if (baseValue > 1.0)
                            baseValue /= 100.0;

                        double scaledValue = baseValue * subStat.Level;
                        breakdown["CRIT Rate"]["Discs_Flat"] += scaledValue;
                    }
                    else if (subStat.Type == StatType.CritDMGFlat)
                    {
                        double baseValue = subStat.Value;
                        if (baseValue > 1.0)
                            baseValue /= 100.0;

                        double scaledValue = baseValue * subStat.Level;
                        breakdown["CRIT DMG"]["Discs_Flat"] += scaledValue;
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

            CalculateFinalValues(breakdown, agent);

            return breakdown;
        }

        private static Dictionary<string, Dictionary<string, double>> InitializeTotalsDictionary()
        {
            var breakdown = new Dictionary<string, Dictionary<string, double>>();
            var statGroups = new List<string> {
                "HP", "ATK", "DEF", "Impact", "CRIT Rate", "CRIT DMG", "Energy Regen",
                "Anomaly Mastery", "Anomaly Proficiency", "Pen Ratio", "PEN",
                "Physical DMG", "Fire DMG", "Ice DMG", "Electric DMG", "Ether DMG",
                "Sheer Force", "Automatic Adrenaline Accumulation",
            };
            foreach (var group in statGroups)
            {
                breakdown[group] = new Dictionary<string, double> {
                    { "Agent_Base", 0 }, { "Agent_Flat", 0 }, { "Agent_Percent", 0 },
                    { "WeaponBase", 0 },{ "Weapon_Flat", 0 }, { "Weapon_Percent", 0 },
                    { "Discs_Flat", 0 }, { "Discs_Percent", 0 },
                    { "SetBonus_Flat", 0 }, { "SetBonus_Percent", 0 },
                    { "Final", 0 }, {"BaseDisplay", 0}, {"AddedDisplay", 0}
                };
            }
            breakdown["CRIT Rate"]["Agent_Base"] = BASE_CRIT_RATE;
            breakdown["CRIT DMG"]["Agent_Base"] = BASE_CRIT_DMG;
            breakdown["Energy Regen"]["Agent_Base"] = BASE_ENERGY_REGEN;
            return breakdown;
        }

        private static void AddContribution(Dictionary<string, Dictionary<string, double>> breakdown, StatType statType, double value, string sourcePrefix)
        {
            string category = GetStatCategory(statType);

            if (string.IsNullOrEmpty(category) || !breakdown.ContainsKey(category)) return;

            string targetBucketSuffix;
            double valueToAdd = value;


            if (category == "CRIT Rate" || category == "CRIT DMG")
            {
                if (sourcePrefix == "Agent" && statType.ToString().EndsWith("Base"))
                {
                    targetBucketSuffix = "Base";
                    valueToAdd = value / 100.0;
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
                        else if (category == "CRIT Rate" || category == "CRIT DMG")
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

        private static void CalculateFinalValues(Dictionary<string, Dictionary<string, double>> breakdown, ZZZAgent agent)
        {
            double calculatedHP = 0;
            double calculatedATK = 0;

            if (agent == null || agent.Stats == null)
            {
                throw new ArgumentNullException(nameof(agent), "Agent data cannot be null for calculating final stats.");
            }

            foreach (var category in breakdown.Keys)
            {
                var catBreakdown = breakdown[category];

                double totalPercentBonus = (catBreakdown["Agent_Percent"] + catBreakdown["Weapon_Percent"] + catBreakdown["Discs_Percent"] + catBreakdown["SetBonus_Percent"]) / 100.0;

                double totalFlatBonus = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                      catBreakdown["Discs_Flat"];

                double agentBase = catBreakdown["Agent_Base"];
                double weaponBase = catBreakdown.ContainsKey("WeaponBase") ? catBreakdown["WeaponBase"] : 0;

                double finalValue = 0;
                double bonusFlat = catBreakdown["SetBonus_Flat"] / 10;

                switch (category)
                {
                    case "HP":
                        double hpSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] * 10.0) : 0;
                        double totalPercentBonusHP = (catBreakdown["Agent_Percent"] + catBreakdown["Weapon_Percent"] + catBreakdown["Discs_Percent"] + hpSetBonusPercent) / 100.0;
                        double percentBonus = agentBase * totalPercentBonusHP;
                        finalValue = Math.Ceiling(agentBase + percentBonus + totalFlatBonus);
                        calculatedHP = finalValue;
                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "ATK":
                        double combinedBaseATK = agentBase + weaponBase;
                        double discPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double weaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double agentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double setBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double atkDiscPercentBonus = combinedBaseATK * discPercentBonus;
                        double atkSetBonusValue = combinedBaseATK * setBonusPercent;
                        double atkAgentWeaponPercentBonus = combinedBaseATK * (agentPercentBonus + weaponPercentBonus);

                        double flatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                           catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double addedValue = atkDiscPercentBonus + atkSetBonusValue + atkAgentWeaponPercentBonus + flatBonuses;
                        finalValue = Math.Floor(combinedBaseATK + addedValue);

                        calculatedATK = finalValue;
                        catBreakdown["BaseDisplay"] = combinedBaseATK;
                        catBreakdown["AddedDisplay"] = finalValue - combinedBaseATK;
                        break;

                    case "DEF":
                        double defDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double defWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double defAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double defSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double defDiscValue = agentBase * defDiscPercentBonus;
                        double defSetValue = agentBase * defSetBonusPercent;
                        double defAgentWeaponValue = agentBase * (defAgentPercentBonus + defWeaponPercentBonus);

                        double defFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                              catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double defAddedValue = defDiscValue + defSetValue + defAgentWeaponValue + defFlatBonuses;
                        finalValue = Math.Floor(agentBase + defAddedValue);

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "Impact":
                        double impactDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double impactWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double impactAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double impactSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double impactDiscValue = agentBase * impactDiscPercentBonus;
                        double impactSetValue = agentBase * impactSetBonusPercent;
                        double impactAgentWeaponValue = agentBase * (impactAgentPercentBonus + impactWeaponPercentBonus);

                        double impactFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                 catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double impactAddedValue = impactDiscValue + impactSetValue + impactAgentWeaponValue + impactFlatBonuses;
                        finalValue = Math.Floor(agentBase + impactAddedValue);

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "Anomaly Mastery":
                        double anomalyMasteryDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double anomalyMasteryWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double anomalyMasteryAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double anomalyMasterySetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double anomalyMasteryDiscValue = agentBase * anomalyMasteryDiscPercentBonus;
                        double anomalyMasterySetValue = agentBase * anomalyMasterySetBonusPercent;
                        double anomalyMasteryAgentWeaponValue = agentBase * (anomalyMasteryAgentPercentBonus + anomalyMasteryWeaponPercentBonus);

                        double anomalyMasteryFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                         catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double anomalyMasteryAddedValue = anomalyMasteryDiscValue + anomalyMasterySetValue + anomalyMasteryAgentWeaponValue + anomalyMasteryFlatBonuses;
                        finalValue = Math.Floor(agentBase + anomalyMasteryAddedValue);

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "Anomaly Proficiency":
                        double anomalyProficiencyDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double anomalyProficiencyWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double anomalyProficiencyAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double anomalyProficiencySetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double anomalyProficiencyDiscValue = agentBase * anomalyProficiencyDiscPercentBonus;
                        double anomalyProficiencySetValue = agentBase * anomalyProficiencySetBonusPercent;
                        double anomalyProficiencyAgentWeaponValue = agentBase * (anomalyProficiencyAgentPercentBonus + anomalyProficiencyWeaponPercentBonus);

                        double anomalyProficiencyFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                             catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double anomalyProficiencyAddedValue = anomalyProficiencyDiscValue + anomalyProficiencySetValue + anomalyProficiencyAgentWeaponValue + anomalyProficiencyFlatBonuses;
                        finalValue = Math.Floor(agentBase + anomalyProficiencyAddedValue);

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "PEN":
                        double penDiscPercentBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] / 100.0 : 0;
                        double penWeaponPercentBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] / 100.0 : 0;
                        double penAgentPercentBonus = catBreakdown["Agent_Percent"] != 0 ? catBreakdown["Agent_Percent"] / 100.0 : 0;
                        double penSetBonusPercent = catBreakdown["SetBonus_Percent"] != 0 ? (catBreakdown["SetBonus_Percent"] / 10.0) : 0;

                        double penDiscValue = agentBase * penDiscPercentBonus;
                        double penSetValue = agentBase * penSetBonusPercent;
                        double penAgentWeaponValue = agentBase * (penAgentPercentBonus + penWeaponPercentBonus);

                        double penFlatBonuses = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                              catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        double penAddedValue = penDiscValue + penSetValue + penAgentWeaponValue + penFlatBonuses;
                        finalValue = Math.Floor(agentBase + penAddedValue);

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "CRIT Rate":
                    case "CRIT DMG":
                        double calculatedCrit = (totalFlatBonus + totalPercentBonus + bonusFlat) * 100;
                        finalValue = agentBase + calculatedCrit;
                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
                        break;

                    case "Energy Regen":
                        double weaponRegenBonus = catBreakdown["Weapon_Percent"] != 0 ? catBreakdown["Weapon_Percent"] : 0;
                        double discRegenBonus = catBreakdown["Discs_Percent"] != 0 ? catBreakdown["Discs_Percent"] : 0;
                        double setBonusRegenBonus = catBreakdown["SetBonus_Percent"] != 0 ? catBreakdown["SetBonus_Percent"] * 10 : 0;
                        double combinedPercentBonus = weaponRegenBonus + discRegenBonus + setBonusRegenBonus;
                        double totalPercentBonusEnergy = 0;
                        if (combinedPercentBonus != 0)
                        {
                            totalPercentBonusEnergy = Math.Max(0, (agentBase * combinedPercentBonus / 100));
                        }

                        double rawFinalValue = agentBase + totalFlatBonus + totalPercentBonusEnergy;

                        finalValue = Math.Floor(rawFinalValue * 100) / 100;

                        catBreakdown["BaseDisplay"] = agentBase;
                        catBreakdown["AddedDisplay"] = finalValue - agentBase;
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
                        catBreakdown["AddedDisplay"] = elemTotalValue;
                        break;

                    case "Sheer Force":
                        double sheerForceEquipmentBonus = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] +
                                                        catBreakdown["Discs_Flat"] + catBreakdown["SetBonus_Flat"];

                        if (agent.Id == 1371)
                        {
                            finalValue = Math.Floor((calculatedATK * 0.30) + (calculatedHP * 0.1) + sheerForceEquipmentBonus);
                        }
                        else
                        {
                            finalValue = Math.Floor((calculatedATK * 0.30) + sheerForceEquipmentBonus);
                        }

                        catBreakdown["BaseDisplay"] = 0;
                        catBreakdown["AddedDisplay"] = finalValue;
                        break;
                    case "Automatic Adrenaline Accumulation":
                        finalValue = agentBase / 100.0;
                        catBreakdown["BaseDisplay"] = agentBase / 100.0;
                        catBreakdown["AddedDisplay"] = 0;
                        break;
                    default:
                        finalValue = Math.Floor(agentBase * (1.0 + totalPercentBonus) + totalFlatBonus);
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
                case StatType.DefBase: case StatType.DefPercent: case StatType.DefFlat: return "DEF";
                case StatType.ImpactBase: case StatType.ImpactPercent: return "Impact";
                case StatType.CritRateBase: case StatType.CritRateFlat: return "CRIT Rate";
                case StatType.CritDMGBase: case StatType.CritDMGFlat: return "CRIT DMG";
                case StatType.EnergyRegenBase: case StatType.EnergyRegenPercent: case StatType.EnergyRegenFlat: return "Energy Regen";
                case StatType.AutomaticAdrenalineAccumulationBase: case StatType.AutomaticAdrenalineAccumulationFlat: case StatType.AutomaticAdrenalineAccumulationPercent: return "Automatic Adrenaline Accumulation";
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
                case StatType.DefBase: case StatType.DefFlat: return "DEF";
                case StatType.ImpactBase: return "Impact";
                case StatType.HPPercent: return "HP%";
                case StatType.ATKPercent: return "ATK%";
                case StatType.DefPercent: return "DEF%";
                case StatType.ImpactPercent: return "Impact";
                case StatType.CritRateBase: case StatType.CritRateFlat: return "CRIT Rate";
                case StatType.CritDMGBase: case StatType.CritDMGFlat: return "CRIT DMG";
                case StatType.EnergyRegenBase: case StatType.EnergyRegenPercent: case StatType.EnergyRegenFlat: return "Energy Regen";
                case StatType.AutomaticAdrenalineAccumulationBase: case StatType.AutomaticAdrenalineAccumulationFlat: case StatType.AutomaticAdrenalineAccumulationPercent: return "Automatic Adrenaline Accumulation";
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
            var breakdownData = CalculateTotalBreakdown(agent, agent.Assets);
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

            if (statGroup == "CRIT Rate")
            {
                result.BaseValue = (BASE_CRIT_RATE * 100);
            }
            else if (statGroup == "CRIT DMG")
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

        public static StatType GetStatTypeFromFriendlyName(string friendlyName, bool isPercentage, bool isEnergyRegen = false)
        {
            if (isEnergyRegen) return StatType.EnergyRegenPercent;
            switch (friendlyName)
            {
                case "HP": return isPercentage ? StatType.HPPercent : StatType.HPFlat;
                case "ATK": return isPercentage ? StatType.ATKPercent : StatType.ATKFlat;
                case "DEF": return isPercentage ? StatType.DefPercent : StatType.DefFlat;
                case "Impact": return isPercentage ? StatType.ImpactPercent : StatType.ImpactBase;
                case "CRIT Rate": return StatType.CritRateFlat;
                case "CRIT DMG": return StatType.CritDMGFlat;
                case "Anomaly Mastery": return StatType.AnomalyMasteryFlat;
                case "Anomaly Proficiency": return StatType.AnomalyProficiencyFlat;
                case "Pen Ratio": return StatType.PenRatioFlat;
                case "PEN": return StatType.PENFlat;
                case "Physical DMG": return StatType.PhysicalDMGBonusFlat;
                case "Fire DMG": return StatType.FireDMGBonusFlat;
                case "Ice DMG": return StatType.IceDMGBonusFlat;
                case "Electric DMG": return StatType.ElectricDMGBonusFlat;
                case "Ether DMG": return StatType.EtherDMGBonusFlat;
                case "Automatic Adrenaline Accumulation": return StatType.AutomaticAdrenalineAccumulationBase;
                default: return StatType.None;
            }
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
        public ZZZStatValue Base => new ZZZStatValue(BaseValue, IsPercentage, StatGroup == "Energy Regen");
        public ZZZStatValue Weapon => new ZZZStatValue(WeaponValue, IsPercentage, StatGroup == "Energy Regen");
        public ZZZStatValue Discs => new ZZZStatValue(DiscsValue, IsPercentage, StatGroup == "Energy Regen");
        public ZZZStatValue SetBonus => new ZZZStatValue(SetBonusValue, IsPercentage, StatGroup == "Energy Regen");
        public ZZZStatValue Total => new ZZZStatValue(TotalValue, IsPercentage, StatGroup == "Energy Regen");
        public double AddedValue => TotalValue - BaseValue;
        public ZZZStatValue Added => new ZZZStatValue(AddedValue, IsPercentage, StatGroup == "Energy Regen");
    }
}