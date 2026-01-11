using System;
using System.Collections.Generic;
using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Utils.HSR
{
    public static class HSRStatPropertyUtils
    {
        private static readonly Dictionary<string, string> DisplayNameToPropertyType = new Dictionary<string, string>
        {
            { "HP", "HPDelta" },
            { "HP%", "HPAddedRatio" },
            { "ATK", "AttackDelta" },
            { "ATK%", "AttackAddedRatio" },
            { "DEF", "DefenceDelta" },
            { "DEF%", "DefenceAddedRatio" },
            { "SPD", "SpeedDelta" },
            { "CRIT Rate", "CriticalChance" },
            { "CRIT DMG", "CriticalDamage" },
            { "Effect Hit Rate", "StatusProbability" },
            { "Effect RES", "StatusResistance" },
            { "Break Effect", "BreakDamageAddedRatio" },
            { "Energy Regeneration Rate", "SPRatioBase" },
            { "Outgoing Healing", "HealRatioBase" }

        };

        private static readonly Dictionary<string, string> PropertyTypeToDisplayName = new Dictionary<string, string>();

        private static readonly Dictionary<string, bool> IsPercentProperty = new Dictionary<string, bool>
        {
            { "HPDelta", false },
            { "HPAddedRatio", true },
            { "AttackDelta", false },
            { "AttackAddedRatio", true },
            { "DefenceDelta", false },
            { "DefenceAddedRatio", true },
            { "SpeedDelta", false },
            { "SpeedAddedRatio", true },
            { "CriticalChance", true },
            { "CriticalChanceBase", true },
            { "CriticalDamage", true },
            { "CriticalDamageBase", true },
            { "StatusProbability", true },
            { "StatusProbabilityBase", true },
            { "StatusResistance", true },
            { "StatusResistanceBase", true },
            { "BreakDamageAddedRatio", true },
            { "BreakDamageAddedRatioBase", true },
            { "HealRatio", true },
            { "HealRatioBase", true },
            { "HealTakenRatio", true },
            { "SPRatio", true },
            { "SPRatioBase", true },
            // Elemental DMG Boost
            { "PhysicalAddedRatio", true },
            { "FireAddedRatio", true },
            { "IceAddedRatio", true },
            { "ThunderAddedRatio", true },
            { "WindAddedRatio", true },
            { "QuantumAddedRatio", true },
            { "ImaginaryAddedRatio", true },
            // Elemental Resistance
            { "PhysicalResistance", true },
            { "FireResistance", true },
            { "IceResistance", true },
            { "ThunderResistance", true },
            { "WindResistance", true },
            { "QuantumResistance", true },
            { "ImaginaryResistance", true }
        };


        private static readonly Dictionary<string, string> FinalStatKeyToDisplayName = new Dictionary<string, string>
        {
            { "HP", "HP" },
            { "Attack", "ATK" },
            { "Defense", "DEF" },
            { "Speed", "SPD" },
            { "CritRate", "CRIT Rate" },
            { "CritDMG", "CRIT DMG" },
            { "BreakEffect", "Break Effect" },
            { "HealingBoost", "Outgoing Healing" },
            { "EnergyRegenRate", "Energy Regeneration Rate" },
            { "EffectHitRate", "Effect Hit Rate" },
            { "EffectResistance", "Effect RES" },
            { "PhysicalDamageBoost", "Physical DMG" },
            { "FireDamageBoost", "Fire DMG" },
            { "IceDamageBoost", "Ice DMG" },
            { "LightningDamageBoost", "Lightning DMG" },
            { "WindDamageBoost", "Wind DMG" },
            { "QuantumDamageBoost", "Quantum DMG" },
            { "ImaginaryDamageBoost", "Imaginary DMG" }
        };


        static HSRStatPropertyUtils()
        {

            foreach (var kvp in DisplayNameToPropertyType)
            {

                if (!PropertyTypeToDisplayName.ContainsKey(kvp.Value))
                {
                    PropertyTypeToDisplayName[kvp.Value] = kvp.Key;
                }
            }


            PropertyTypeToDisplayName["CriticalChanceBase"] = "CRIT Rate";
            PropertyTypeToDisplayName["CriticalDamageBase"] = "CRIT DMG";
            PropertyTypeToDisplayName["BreakDamageAddedRatioBase"] = "Break Effect";
            PropertyTypeToDisplayName["SPRatioBase"] = "Energy Regeneration Rate";
            PropertyTypeToDisplayName["HealRatioBase"] = "Outgoing Healing";
            PropertyTypeToDisplayName["StatusProbability"] = "Effect Hit Rate";
            PropertyTypeToDisplayName["StatusResistance"] = "Effect RES";


            PropertyTypeToDisplayName["PhysicalAddedRatio"] = "Physical DMG";
            PropertyTypeToDisplayName["FireAddedRatio"] = "Fire DMG";
            PropertyTypeToDisplayName["IceAddedRatio"] = "Ice DMG";
            PropertyTypeToDisplayName["ThunderAddedRatio"] = "Lightning DMG";
            PropertyTypeToDisplayName["WindAddedRatio"] = "Wind DMG";
            PropertyTypeToDisplayName["QuantumAddedRatio"] = "Quantum DMG";
            PropertyTypeToDisplayName["ImaginaryAddedRatio"] = "Imaginary DMG";


            PropertyTypeToDisplayName["HPDelta"] = "HP";
            PropertyTypeToDisplayName["HPAddedRatio"] = "HP%";
            PropertyTypeToDisplayName["AttackDelta"] = "ATK";
            PropertyTypeToDisplayName["AttackAddedRatio"] = "ATK%";
            PropertyTypeToDisplayName["DefenceDelta"] = "DEF";
            PropertyTypeToDisplayName["DefenceAddedRatio"] = "DEF%";
            PropertyTypeToDisplayName["SpeedDelta"] = "SPD";
            PropertyTypeToDisplayName["StatusResistanceBase"] = "Effect RES";
            PropertyTypeToDisplayName["SpeedAddedRatio"] = "SPD";


            PropertyTypeToDisplayName["BaseHP"] = "Base HP";
            PropertyTypeToDisplayName["BaseAttack"] = "Base ATK";
            PropertyTypeToDisplayName["BaseDefence"] = "Base DEF";
            PropertyTypeToDisplayName["BaseSpeed"] = "Base SPD";
        }

        public static bool IsPercentageType(string propertyType)
        {

            return (IsPercentProperty.TryGetValue(propertyType, out bool isPercent) && isPercent)
                   || (PropertyTypeToDisplayName.TryGetValue(propertyType, out var name) && name.EndsWith("%"));
        }


        public static string GetDisplayName(string propertyType)
        {
            return PropertyTypeToDisplayName.TryGetValue(propertyType, out string displayName)
                ? displayName
                : propertyType;
        }


        public static string GetFinalStatDisplayName(string finalStatKey)
        {
            return FinalStatKeyToDisplayName.TryGetValue(finalStatKey, out string displayName)
               ? displayName
               : finalStatKey;
        }


        public static string GetPropertyType(string displayName)
        {
            return DisplayNameToPropertyType.TryGetValue(displayName, out string propertyType)
                ? propertyType
                : displayName;
        }

        public static string FormatPropertyValue(string propertyType, double value)
        {
            bool isPercent = IsPercentageType(propertyType);

            if (isPercent)
            {


                double displayValue = value > 1.0 ? value : value * 100;
                return $"{displayValue:F1}%";
            }
            else if (propertyType == "SpeedDelta" || propertyType == "BaseSpeed")
            {

                return $"{value:F1}";
            }
            else
            {

                return $"{(int)Math.Floor(value)}";
            }
        }

        public static double ConvertToCalculationValue(string propertyType, double displayValue)
        {
            bool isPercent = IsPercentageType(propertyType);

            if (isPercent)
            {

                return displayValue / 100.0;
            }

            return displayValue;
        }

        public static StatPropertyType MapToStatPropertyType(string propertyType)
        {
            switch (propertyType)
            {
                // Final Stats
                case "MaxHP": return StatPropertyType.MaxHP;
                case "Attack": return StatPropertyType.Attack;
                case "Defence": return StatPropertyType.Defence;
                case "Speed": return StatPropertyType.Speed;

                // Base Stats
                case "BaseHP": return StatPropertyType.BaseHP;
                case "BaseAttack": return StatPropertyType.BaseAttack;
                case "BaseDefence": return StatPropertyType.BaseDefence;
                case "BaseSpeed": return StatPropertyType.BaseSpeed;

                // Delta Stats
                case "HPDelta": return StatPropertyType.HPDelta;
                case "AttackDelta": return StatPropertyType.AttackDelta;
                case "DefenceDelta": return StatPropertyType.DefenceDelta;
                case "SpeedDelta": return StatPropertyType.SpeedDelta;

                // Ratio Stats
                case "HPAddedRatio": return StatPropertyType.HPAddedRatio;
                case "AttackAddedRatio": return StatPropertyType.AttackAddedRatio;
                case "DefenceAddedRatio": return StatPropertyType.DefenceAddedRatio;
                case "SpeedAddedRatio": return StatPropertyType.SpeedAddedRatio;

                // Critical Stats
                case "CriticalChance": return StatPropertyType.CriticalChance;
                case "CriticalDamage": return StatPropertyType.CriticalDamage;
                case "CriticalChanceBase": return StatPropertyType.CriticalChanceBase;
                case "CriticalDamageBase": return StatPropertyType.CriticalDamageBase;

                // Effect Stats
                case "StatusProbability": return StatPropertyType.StatusProbability;
                case "StatusResistance": return StatPropertyType.StatusResistance;
                case "StatusProbabilityBase": return StatPropertyType.StatusProbabilityBase;
                case "StatusResistanceBase": return StatPropertyType.StatusResistanceBase;

                // Break Effect
                case "BreakDamageAddedRatio": return StatPropertyType.BreakDamageAddedRatio;
                case "BreakDamageAddedRatioBase": return StatPropertyType.BreakDamageAddedRatioBase;

                // Healing Stats
                case "HealRatio": return StatPropertyType.HealRatio;
                case "HealRatioBase": return StatPropertyType.HealRatioBase;
                case "HealTakenRatio": return StatPropertyType.HealTakenRatio;

                // Energy Stats
                case "MaxSP": return StatPropertyType.MaxSP;
                case "SPRatio": return StatPropertyType.SPRatio;
                case "SPRatioBase": return StatPropertyType.SPRatioBase;

                // Elemental DMG Boost
                case "PhysicalAddedRatio": return StatPropertyType.PhysicalAddedRatio;
                case "FireAddedRatio": return StatPropertyType.FireAddedRatio;
                case "IceAddedRatio": return StatPropertyType.IceAddedRatio;
                case "ThunderAddedRatio": return StatPropertyType.LightningAddedRatio;
                case "WindAddedRatio": return StatPropertyType.WindAddedRatio;
                case "QuantumAddedRatio": return StatPropertyType.QuantumAddedRatio;
                case "ImaginaryAddedRatio": return StatPropertyType.ImaginaryAddedRatio;

                // Elemental Resistance
                case "PhysicalResistance": return StatPropertyType.PhysicalResistance;
                case "FireResistance": return StatPropertyType.FireResistance;
                case "IceResistance": return StatPropertyType.IceResistance;
                case "ThunderResistance": return StatPropertyType.LightningResistance;
                case "WindResistance": return StatPropertyType.WindResistance;
                case "QuantumResistance": return StatPropertyType.QuantumResistance;
                case "ImaginaryResistance": return StatPropertyType.ImaginaryResistance;

                // Elemental Resistance Delta
                case "PhysicalResistanceDelta": return StatPropertyType.PhysicalResistanceDelta;
                case "FireResistanceDelta": return StatPropertyType.FireResistanceDelta;
                case "IceResistanceDelta": return StatPropertyType.IceResistanceDelta;
                case "ThunderResistanceDelta": return StatPropertyType.LightningResistanceDelta;
                case "WindResistanceDelta": return StatPropertyType.WindResistanceDelta;
                case "QuantumResistanceDelta": return StatPropertyType.QuantumResistanceDelta;
                case "ImaginaryResistanceDelta": return StatPropertyType.ImaginaryResistanceDelta;

                default: return StatPropertyType.None;
            }
        }



        public static double GetPropertyScalingFactor(string propertyType)
        {
            return IsPercentageType(propertyType) ? 100.0 : 1.0;
        }
    }
}