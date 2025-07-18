using System;
using System.Collections.Generic;
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
            var result = new Dictionary<string, StatSummary>();
            foreach (var kvp in breakdown)
            {
                result[kvp.Key] = new StatSummary
                {
                    FinalValue = kvp.Value.TryGetValue("Final", out var final) ? final : 0,
                    BaseValue = kvp.Value.TryGetValue("BaseDisplay", out var baseVal) ? baseVal : 0,
                    AddedValue = kvp.Value.TryGetValue("AddedDisplay", out var addedVal) ? addedVal : 0
                };
            }
            return result;
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

        internal static Dictionary<string, Dictionary<string, double>> CalculateTotalBreakdown(ZZZAgent agent, IZZZAssets assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            var breakdown = new Dictionary<string, Dictionary<string, double>>();

            foreach (var stat in agent.Stats)
            {
                AddContribution(breakdown, stat.Key, stat.Value, "Agent");
            }

            if (agent.Weapon != null)
            {
                if (agent.Weapon.MainStat.Type == StatType.ATKBase)
                {
                    GetOrCreateCategory(breakdown, "ATK")["WeaponBase"] += agent.Weapon.MainStat.Value;
                }

                if (agent.Weapon.SecondaryStat.Type == StatType.CritRateFlat)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    if (value > 1.0) value /= 100.0;
                    GetOrCreateCategory(breakdown, "CRIT Rate")["Weapon_Flat"] += value;
                }
                else if (agent.Weapon.SecondaryStat.Type == StatType.CritDMGFlat)
                {
                    double value = agent.Weapon.SecondaryStat.Value;
                    if (value > 1.0) value /= 100.0;
                    GetOrCreateCategory(breakdown, "CRIT DMG")["Weapon_Flat"] += value;
                }
                else if (agent.Weapon.SecondaryStat.Type == StatType.EnergyRegenPercent)
                {
                    GetOrCreateCategory(breakdown, "Energy Regen")["Weapon_Percent"] += agent.Weapon.SecondaryStat.Value;
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
                    if (value > 1.0) value /= 100.0;
                    GetOrCreateCategory(breakdown, "CRIT Rate")["Discs_Flat"] += value;
                }
                else if (disc.MainStat.Type == StatType.CritDMGFlat)
                {
                    double value = disc.MainStat.Value;
                    if (value > 1.0) value /= 100.0;
                    GetOrCreateCategory(breakdown, "CRIT DMG")["Discs_Flat"] += value;
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
                        if (baseValue > 1.0) baseValue /= 100.0;
                        GetOrCreateCategory(breakdown, "CRIT Rate")["Discs_Flat"] += baseValue * subStat.Level;
                    }
                    else if (subStat.Type == StatType.CritDMGFlat)
                    {
                        double baseValue = subStat.Value;
                        if (baseValue > 1.0) baseValue /= 100.0;
                        GetOrCreateCategory(breakdown, "CRIT DMG")["Discs_Flat"] += baseValue * subStat.Level;
                    }
                    else
                    {
                        AddContribution(breakdown, subStat.Type, subStat.Value * subStat.Level, "Discs");
                    }
                }
            }

            if (!breakdown.ContainsKey("Sheer Force"))
            {
                GetOrCreateCategory(breakdown, "Sheer Force");
            }

            ApplySetBonuses(agent, breakdown, assets);
            CalculateFinalValues(breakdown, agent);
            return breakdown;
        }

        private static Dictionary<string, double> GetOrCreateCategory(Dictionary<string, Dictionary<string, double>> breakdown, string category)
        {
            if (!breakdown.TryGetValue(category, out var subDict))
            {
                subDict = new Dictionary<string, double>
                {
                    { "Agent_Base", 0 }, { "Agent_Flat", 0 }, { "Agent_Percent", 0 },
                    { "WeaponBase", 0 },{ "Weapon_Flat", 0 }, { "Weapon_Percent", 0 },
                    { "Discs_Flat", 0 }, { "Discs_Percent", 0 },
                    { "SetBonus_Flat", 0 }, { "SetBonus_Percent", 0 },
                    { "Final", 0 }, {"BaseDisplay", 0}, {"AddedDisplay", 0}
                };
                breakdown[category] = subDict;

                if (category == "CRIT Rate") subDict["Agent_Base"] = BASE_CRIT_RATE;
                else if (category == "CRIT DMG") subDict["Agent_Base"] = BASE_CRIT_DMG;
                else if (category == "Energy Regen") subDict["Agent_Base"] = BASE_ENERGY_REGEN;
            }
            return subDict;
        }

        private static void AddContribution(Dictionary<string, Dictionary<string, double>> breakdown, StatType statType, double value, string sourcePrefix)
        {
            string category = GetStatCategory(statType);
            if (string.IsNullOrEmpty(category)) return;

            var categoryDict = GetOrCreateCategory(breakdown, category);
            string targetBucketSuffix;
            double valueToAdd = value;

            if (category == "CRIT Rate" || category == "CRIT DMG")
            {
                if (sourcePrefix == "Agent" && statType.ToString().EndsWith("Base"))
                {
                    targetBucketSuffix = "Base";
                    valueToAdd = value / 100.0;
                }
                else
                {
                    targetBucketSuffix = "Flat";
                    if (sourcePrefix != "Agent" && value > 1.0)
                    {
                        valueToAdd = value / 100.0;
                    }
                }
            }
            else
            {
                bool isPercentForCalc = IsCalculationPercentageStat(statType);
                if (sourcePrefix == "Agent")
                {
                    if (statType.ToString().EndsWith("Base"))
                    {
                        if (statType == StatType.EnergyRegenBase) valueToAdd = value / 100;
                        targetBucketSuffix = "Base";
                    }
                    else
                    {
                        targetBucketSuffix = isPercentForCalc ? "Percent" : "Flat";
                        if (isPercentForCalc) valueToAdd = value / 100.0;
                    }
                }
                else
                {
                    if (isPercentForCalc)
                    {
                        if (sourcePrefix == "SetBonus") valueToAdd = value / 100.0;
                        targetBucketSuffix = (statType == StatType.EnergyRegenFlat || statType == StatType.PenRatioFlat || statType.ToString().Contains("DMGBonusFlat")) ? "Flat" : "Percent";
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

            if (!categoryDict.ContainsKey(bucketKey))
            {
                categoryDict[bucketKey] = 0;
            }
            categoryDict[bucketKey] += valueToAdd;
        }

        private static void ApplySetBonuses(ZZZAgent agent, Dictionary<string, Dictionary<string, double>> breakdown, IZZZAssets assets)
        {
            var equippedSets = new Dictionary<int, int>();
            foreach (var disc in agent.EquippedDiscs)
            {
                if (!equippedSets.ContainsKey(disc.SuitId)) equippedSets[disc.SuitId] = 0;
                equippedSets[disc.SuitId]++;
            }

            foreach (var set in equippedSets)
            {
                if (set.Value < 2) continue;

                var suitInfo = assets.GetDiscSetInfo(set.Key.ToString());
                if (suitInfo?.SetBonusProps == null || suitInfo.SetBonusProps.Count == 0) continue;

                foreach (var prop in suitInfo.SetBonusProps)
                {
                    if (int.TryParse(prop.Key, out int propertyId) && Enum.IsDefined(typeof(StatType), propertyId))
                    {
                        StatType statType = (StatType)propertyId;
                        double rawValue = prop.Value;
                        string category = GetStatCategory(statType);
                        var categoryDict = GetOrCreateCategory(breakdown, category);

                        if (statType == StatType.EnergyRegenPercent)
                        {
                            categoryDict["SetBonus_Percent"] += rawValue / 1000.0;
                        }
                        else if (category == "CRIT Rate" || category == "CRIT DMG")
                        {
                            categoryDict["SetBonus_Flat"] += rawValue / 1000.0;
                        }
                        else if (IsCalculationPercentageStat(statType))
                        {
                            categoryDict["SetBonus_Percent"] += rawValue / 1000.0;
                        }
                        else
                        {
                            categoryDict["SetBonus_Flat"] += rawValue;
                        }
                    }
                }
            }
        }

        private static void CalculateFinalValues(Dictionary<string, Dictionary<string, double>> breakdown, ZZZAgent agent)
        {
            if (agent == null) throw new ArgumentNullException(nameof(agent));

            double calculatedHP = 0;
            double calculatedATK = 0;

            if (breakdown.TryGetValue("HP", out var hpCat))
            {
                double hpSetBonusPercent = hpCat["SetBonus_Percent"] * 10.0;
                double totalPercentBonusHP = (hpCat["Agent_Percent"] + hpCat["Weapon_Percent"] + hpCat["Discs_Percent"] + hpSetBonusPercent) / 100.0;
                calculatedHP = Math.Ceiling(hpCat["Agent_Base"] * (1 + totalPercentBonusHP) + hpCat["Agent_Flat"] + hpCat["Weapon_Flat"] + hpCat["Discs_Flat"]);
                hpCat["Final"] = calculatedHP;
                hpCat["BaseDisplay"] = hpCat["Agent_Base"];
                hpCat["AddedDisplay"] = calculatedHP - hpCat["Agent_Base"];
            }

            if (breakdown.TryGetValue("ATK", out var atkCat))
            {
                double combinedBaseATK = atkCat["Agent_Base"] + atkCat["WeaponBase"];
                double setBonusPercent = atkCat["SetBonus_Percent"] / 10.0;
                double totalPercentBonus = (atkCat["Agent_Percent"] + atkCat["Weapon_Percent"] + atkCat["Discs_Percent"]) / 100.0 + setBonusPercent;
                double flatBonuses = atkCat["Agent_Flat"] + atkCat["Weapon_Flat"] + atkCat["Discs_Flat"] + atkCat["SetBonus_Flat"];
                calculatedATK = Math.Floor(combinedBaseATK * (1 + totalPercentBonus) + flatBonuses);
                atkCat["Final"] = calculatedATK;
                atkCat["BaseDisplay"] = combinedBaseATK;
                atkCat["AddedDisplay"] = calculatedATK - combinedBaseATK;
            }

            foreach (var entry in breakdown)
            {
                string category = entry.Key;
                var catBreakdown = entry.Value;
                if (category == "HP" || category == "ATK") continue;

                double agentBase = catBreakdown["Agent_Base"];
                double totalPercentBonus = (catBreakdown["Agent_Percent"] + catBreakdown["Weapon_Percent"] + catBreakdown["Discs_Percent"] + catBreakdown["SetBonus_Percent"]) / 100.0;
                double totalFlatBonus = catBreakdown["Agent_Flat"] + catBreakdown["Weapon_Flat"] + catBreakdown["Discs_Flat"];
                double finalValue = 0;

                switch (category)
                {
                    case "DEF":
                    case "Impact":
                    case "Anomaly Mastery":
                    case "Anomaly Proficiency":
                    case "PEN":
                        finalValue = Math.Floor(agentBase * (1 + totalPercentBonus) + totalFlatBonus);
                        break;
                    case "CRIT Rate":
                    case "CRIT DMG":
                        finalValue = agentBase + (totalFlatBonus + totalPercentBonus + (catBreakdown["SetBonus_Flat"] / 10)) * 100;
                        break;
                    case "Energy Regen":
                        double combinedPercentBonus = catBreakdown["Weapon_Percent"] + catBreakdown["Discs_Percent"] + (catBreakdown["SetBonus_Percent"] * 10);
                        double totalPercentBonusEnergy = (combinedPercentBonus > 0) ? Math.Max(0, (agentBase * combinedPercentBonus / 100)) : 0;
                        finalValue = Math.Floor((agentBase + totalFlatBonus + totalPercentBonusEnergy) * 100) / 100;
                        break;
                    case "Pen Ratio":
                        finalValue = (totalPercentBonus * 1000) + totalFlatBonus;
                        break;
                    case "Physical DMG":
                    case "Fire DMG":
                    case "Ice DMG":
                    case "Electric DMG":
                    case "Ether DMG":
                        finalValue = totalPercentBonus + totalFlatBonus;
                        break;
                    case "Sheer Force":
                        if (agent.ProfessionType == ProfessionType.Rupture)
                        {
                            finalValue = (agent.Id == 1371)
                                ? Math.Floor(calculatedATK * 0.30) + Math.Floor(calculatedHP * 0.1) + totalFlatBonus
                                : Math.Floor(calculatedATK * 0.30) + totalFlatBonus;
                        }
                        break;
                    case "Automatic Adrenaline Accumulation":
                        finalValue = agentBase / 100.0;
                        break;
                    default:
                        finalValue = Math.Floor(agentBase * (1 + totalPercentBonus) + totalFlatBonus);
                        break;
                }

                catBreakdown["Final"] = finalValue;
                if (category != "Sheer Force")
                {
                    catBreakdown["BaseDisplay"] = agentBase;
                    catBreakdown["AddedDisplay"] = finalValue - agentBase;
                }
                else
                {
                    catBreakdown["BaseDisplay"] = 0;
                    catBreakdown["AddedDisplay"] = finalValue;
                }
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
                default:
                    return false;
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