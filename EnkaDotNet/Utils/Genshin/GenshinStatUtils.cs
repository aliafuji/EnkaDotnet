using EnkaDotNet.Enums.Genshin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace EnkaDotNet.Utils.Genshin
{
    public static class GenshinStatUtils
    {
        private static readonly Dictionary<StatType, string> StatTypeToDisplayName = new Dictionary<StatType, string>
        {
            { StatType.BaseHP, "Base HP" },
            { StatType.HP, "HP" },
            { StatType.HPPercentage, "HP%" },
            { StatType.BaseAttack, "Base ATK" },
            { StatType.Attack, "ATK" },
            { StatType.AttackPercentage, "ATK%" },
            { StatType.BaseDefense, "Base DEF" },
            { StatType.Defense, "DEF" },
            { StatType.DefensePercentage, "DEF%" },
            { StatType.BaseSpeed, "Base SPD" },
            { StatType.Speed, "SPD" },
            { StatType.SpeedPercentage, "SPD%" },
            { StatType.CriticalRate, "CRIT Rate" },
            { StatType.CriticalDamage, "CRIT DMG" },
            { StatType.EnergyRecharge, "Energy Recharge" },
            { StatType.ElementalMastery, "Elemental Mastery" },
            { StatType.HealingBonus, "Healing Bonus" },
            { StatType.IncomingHealingBonus, "Incoming Healing Bonus" },
            { StatType.PhysicalResistance, "Physical RES" },
            { StatType.PyroResistance, "Pyro RES" },
            { StatType.ElectroResistance, "Electro RES" },
            { StatType.HydroResistance, "Hydro RES" },
            { StatType.DendroResistance, "Dendro RES" },
            { StatType.AnemoResistance, "Anemo RES" },
            { StatType.GeoResistance, "Geo RES" },
            { StatType.CryoResistance, "Cryo RES" },
            { StatType.PhysicalDamageBonus, "Physical DMG Bonus" },
            { StatType.PyroDamageBonus, "Pyro DMG Bonus" },
            { StatType.ElectroDamageBonus, "Electro DMG Bonus" },
            { StatType.HydroDamageBonus, "Hydro DMG Bonus" },
            { StatType.DendroDamageBonus, "Dendro DMG Bonus" },
            { StatType.AnemoDamageBonus, "Anemo DMG Bonus" },
            { StatType.GeoDamageBonus, "Geo DMG Bonus" },
            { StatType.CryoDamageBonus, "Cryo DMG Bonus" },
            { StatType.CooldownReduction, "CD Reduction" },
            { StatType.ShieldStrength, "Shield Strength" },
            { StatType.CurrentHP, "Current HP" },
            { StatType.CurrentPyroEnergy, "Current Pyro Energy" },
            { StatType.CurrentElectroEnergy, "Current Electro Energy" },
            { StatType.CurrentHydroEnergy, "Current Hydro Energy" },
            { StatType.CurrentDendroEnergy, "Current Dendro Energy" },
            { StatType.CurrentAnemoEnergy, "Current Anemo Energy" },
            { StatType.CurrentCryoEnergy, "Current Cryo Energy" },
            { StatType.CurrentGeoEnergy, "Current Geo Energy" },
            { StatType.CurrentSpecialEnergy, "Current Special Energy" }
        };

        public static string GetDisplayName(StatType statType)
        {
            return StatTypeToDisplayName.TryGetValue(statType, out var name) ? name : statType.ToString();
        }

        public static bool IsPercentage(StatType statType)
        {
            string name = statType.ToString();
            return name.Contains("Percentage") ||
                   name.Contains("Bonus") ||
                   name.Contains("Resistance") ||
                   statType == StatType.CriticalRate ||
                   statType == StatType.CriticalDamage ||
                   statType == StatType.EnergyRecharge ||
                   statType == StatType.CooldownReduction ||
                   statType == StatType.ShieldStrength;
        }

        public static string FormatValue(StatType statType, double value, bool raw)
        {
            if (raw)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            bool isPercent = IsPercentage(statType);
            if (isPercent)
            {
                return (value).ToString("F1", CultureInfo.InvariantCulture) + "%";
            }
            else
            {
                return Math.Round(value).ToString("N0", CultureInfo.InvariantCulture);
            }
        }

        public static string FormatValueStats(StatType statType, double value, bool raw)
        {
            if (raw)
            {
                return value.ToString(CultureInfo.InvariantCulture);
            }

            bool isPercent = IsPercentage(statType);
            if (isPercent)
            {
                if (statType == StatType.EnergyRecharge ||
                    statType == StatType.CooldownReduction ||
                    statType == StatType.CriticalRate ||
                    statType == StatType.CriticalDamage ||
                    statType == StatType.IncomingHealingBonus)
                {
                    if (statType == StatType.CriticalDamage && value < 50)
                    {
                        value *= 100;
                    }
                    else if (statType == StatType.CriticalRate && value <= 5)
                    {
                        value *= 100;
                    }
                    else if (value <= 1)
                    {
                        value *= 100;
                    }
                    return (value).ToString("F1", CultureInfo.InvariantCulture) + "%";
                }
                else if (statType == StatType.ShieldStrength)
                {
                    return Math.Round(value).ToString("N0", CultureInfo.InvariantCulture) + " HP";
                }
                else if (statType == StatType.BaseHP || statType == StatType.HP || statType == StatType.CurrentHP)
                {
                    return Math.Round(value).ToString("N0", CultureInfo.InvariantCulture) + " HP";
                }

                return value.ToString("F1", CultureInfo.InvariantCulture) + "%";
            }
            else
            {
                return Math.Round(value).ToString("N0", CultureInfo.InvariantCulture);
            }
        }
    }
}