using EnkaDotNet.Assets.Genshin;
using EnkaDotNet.Enums.Genshin;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EnkaDotNet.Utils.Genshin
{
    public static class GenshinStatUtils
    {
        /// <summary>
        /// Maps StatType enum values to their text map keys for localization.
        /// </summary>
        private static readonly Dictionary<StatType, string> StatTypeToFightPropKey = new Dictionary<StatType, string>
        {
            { StatType.BaseHP, "FIGHT_PROP_BASE_HP" },
            { StatType.HP_Flat, "FIGHT_PROP_HP" },
            { StatType.HP, "FIGHT_PROP_MAX_HP" },
            { StatType.HPPercentage, "FIGHT_PROP_HP_PERCENT" },
            { StatType.BaseAttack, "FIGHT_PROP_BASE_ATTACK" },
            { StatType.Attack_Flat, "FIGHT_PROP_ATTACK" },
            { StatType.Attack, "FIGHT_PROP_CUR_ATTACK" },
            { StatType.AttackPercentage, "FIGHT_PROP_ATTACK_PERCENT" },
            { StatType.BaseDefense, "FIGHT_PROP_BASE_DEFENSE" },
            { StatType.Defense_Flat, "FIGHT_PROP_DEFENSE" },
            { StatType.Defense, "FIGHT_PROP_CUR_DEFENSE" },
            { StatType.DefensePercentage, "FIGHT_PROP_DEFENSE_PERCENT" },
            { StatType.BaseSpeed, "FIGHT_PROP_BASE_SPEED" },
            { StatType.Speed, "FIGHT_PROP_CUR_SPEED" },
            { StatType.SpeedPercentage, "FIGHT_PROP_SPEED_PERCENT" },
            { StatType.CriticalRate, "FIGHT_PROP_CRITICAL" },
            { StatType.AntiCritical, "FIGHT_PROP_ANTI_CRITICAL" },
            { StatType.CriticalDamage, "FIGHT_PROP_CRITICAL_HURT" },
            { StatType.EnergyRecharge, "FIGHT_PROP_CHARGE_EFFICIENCY" },
            { StatType.AddHurt, "FIGHT_PROP_ADD_HURT" },
            { StatType.SubHurt, "FIGHT_PROP_SUB_HURT" },
            { StatType.HealingBonus, "FIGHT_PROP_HEAL_ADD" },
            { StatType.IncomingHealingBonus, "FIGHT_PROP_HEALED_ADD" },
            { StatType.ElementalMastery, "FIGHT_PROP_ELEMENT_MASTERY" },
            { StatType.PhysicalResistance, "FIGHT_PROP_PHYSICAL_SUB_HURT" },
            { StatType.PhysicalDamageBonus, "FIGHT_PROP_PHYSICAL_ADD_HURT" },
            { StatType.DefenseIgnoreRatio, "FIGHT_PROP_DEFENCE_IGNORE_RATIO" },
            { StatType.DefenseIgnoreDelta, "FIGHT_PROP_DEFENCE_IGNORE_DELTA" },
            { StatType.PyroDamageBonus, "FIGHT_PROP_FIRE_ADD_HURT" },
            { StatType.ElectroDamageBonus, "FIGHT_PROP_ELEC_ADD_HURT" },
            { StatType.HydroDamageBonus, "FIGHT_PROP_WATER_ADD_HURT" },
            { StatType.DendroDamageBonus, "FIGHT_PROP_GRASS_ADD_HURT" },
            { StatType.AnemoDamageBonus, "FIGHT_PROP_WIND_ADD_HURT" },
            { StatType.GeoDamageBonus, "FIGHT_PROP_ROCK_ADD_HURT" },
            { StatType.CryoDamageBonus, "FIGHT_PROP_ICE_ADD_HURT" },
            { StatType.HeadAddHurt, "FIGHT_PROP_HIT_HEAD_ADD_HURT" },
            { StatType.PyroResistance, "FIGHT_PROP_FIRE_SUB_HURT" },
            { StatType.ElectroResistance, "FIGHT_PROP_ELEC_SUB_HURT" },
            { StatType.HydroResistance, "FIGHT_PROP_WATER_SUB_HURT" },
            { StatType.DendroResistance, "FIGHT_PROP_GRASS_SUB_HURT" },
            { StatType.AnemoResistance, "FIGHT_PROP_WIND_SUB_HURT" },
            { StatType.GeoResistance, "FIGHT_PROP_ROCK_SUB_HURT" },
            { StatType.CryoResistance, "FIGHT_PROP_ICE_SUB_HURT" },
            { StatType.EffectHit, "FIGHT_PROP_EFFECT_HIT" },
            { StatType.EffectResist, "FIGHT_PROP_EFFECT_RESIST" },
            { StatType.FreezeResist, "FIGHT_PROP_FREEZE_RESIST" },
            { StatType.DizzyResist, "FIGHT_PROP_DIZZY_RESIST" },
            { StatType.FreezeShorten, "FIGHT_PROP_FREEZE_SHORTEN" },
            { StatType.DizzyShorten, "FIGHT_PROP_DIZZY_SHORTEN" },
            { StatType.CooldownReduction, "FIGHT_PROP_SKILL_CD_MINUS_RATIO" },
            { StatType.ShieldStrength, "FIGHT_PROP_SHIELD_COST_MINUS_RATIO" },
            { StatType.CurrentHP, "FIGHT_PROP_CUR_HP" },
            { StatType.ElementalReactionCritRate, "FIGHT_PROP_ELEM_REACT_CRITICAL" },
            { StatType.ElementalReactionCritDamage, "FIGHT_PROP_ELEM_REACT_CRITICAL_HURT" }
        };

        /// <summary>
        /// Gets the localized display name for a stat type using the text map.
        /// Falls back to the hardcoded English name if the localized text is not found.
        /// </summary>
        public static string GetDisplayName(StatType statType, IGenshinAssets assets)
        {
            if (assets != null && StatTypeToFightPropKey.TryGetValue(statType, out string fightPropKey))
            {
                string localized = assets.GetText(fightPropKey);
                if (!string.IsNullOrEmpty(localized) && localized != fightPropKey)
                {
                    return localized;
                }
            }
            return GetDisplayName(statType);
        }

        private static readonly Dictionary<StatType, string> StatTypeToDisplayName = new Dictionary<StatType, string>
        {
            { StatType.BaseHP, "Base HP" },
            { StatType.HP_Flat, "HP" },
            { StatType.HP, "Max HP" },
            { StatType.HPPercentage, "HP%" },
            { StatType.BaseAttack, "Base ATK" },
            { StatType.Attack_Flat, "ATK" },
            { StatType.Attack, "ATK" },
            { StatType.AttackPercentage, "ATK%" },
            { StatType.BaseDefense, "Base DEF" },
            { StatType.Defense_Flat, "DEF" },
            { StatType.Defense, "DEF" },
            { StatType.DefensePercentage, "DEF%" },
            { StatType.BaseSpeed, "Base SPD" },
            { StatType.Speed, "SPD" },
            { StatType.SpeedPercentage, "SPD%" },
            { StatType.CriticalRate, "CRIT Rate" },
            { StatType.AntiCritical, "Anti-CRIT" },
            { StatType.CriticalDamage, "CRIT DMG" },
            { StatType.EnergyRecharge, "Energy Recharge" },
            { StatType.AddHurt, "DMG Bonus" },
            { StatType.SubHurt, "DMG Reduction" },
            { StatType.HealingBonus, "Healing Bonus" },
            { StatType.IncomingHealingBonus, "Incoming Healing Bonus" },
            { StatType.ElementalMastery, "Elemental Mastery" },
            { StatType.PhysicalResistance, "Physical RES" },
            { StatType.PhysicalDamageBonus, "Physical DMG Bonus" },
            { StatType.DefenseIgnoreRatio, "DEF Ignore%" },
            { StatType.DefenseIgnoreDelta, "DEF Ignore" },
            { StatType.PyroDamageBonus, "Pyro DMG Bonus" },
            { StatType.ElectroDamageBonus, "Electro DMG Bonus" },
            { StatType.HydroDamageBonus, "Hydro DMG Bonus" },
            { StatType.DendroDamageBonus, "Dendro DMG Bonus" },
            { StatType.AnemoDamageBonus, "Anemo DMG Bonus" },
            { StatType.GeoDamageBonus, "Geo DMG Bonus" },
            { StatType.CryoDamageBonus, "Cryo DMG Bonus" },
            { StatType.HeadAddHurt, "Headshot DMG Bonus" },
            { StatType.PyroResistance, "Pyro RES" },
            { StatType.ElectroResistance, "Electro RES" },
            { StatType.HydroResistance, "Hydro RES" },
            { StatType.DendroResistance, "Dendro RES" },
            { StatType.AnemoResistance, "Anemo RES" },
            { StatType.GeoResistance, "Geo RES" },
            { StatType.CryoResistance, "Cryo RES" },
            { StatType.EffectHit, "Effect Hit Rate" },
            { StatType.EffectResist, "Effect RES" },
            { StatType.FreezeResist, "Freeze RES" },
            { StatType.DizzyResist, "Dizzy RES" },
            { StatType.MaxPyroEnergy, "Max Pyro Energy" },
            { StatType.MaxElectroEnergy, "Max Electro Energy" },
            { StatType.MaxHydroEnergy, "Max Hydro Energy" },
            { StatType.MaxDendroEnergy, "Max Dendro Energy" },
            { StatType.MaxAnemoEnergy, "Max Anemo Energy" },
            { StatType.MaxCryoEnergy, "Max Cryo Energy" },
            { StatType.MaxGeoEnergy, "Max Geo Energy" },
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
            { StatType.ElementalReactionCritRate, "Reaction CRIT Rate" },
            { StatType.ElementalReactionCritDamage, "Reaction CRIT DMG" }
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