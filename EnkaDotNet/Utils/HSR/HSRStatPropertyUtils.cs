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
            { "Outgoing Healing", "HealRatioBase" },
            { "Physical DMG", "PhysicalAddedRatio" },
            { "Fire DMG", "FireAddedRatio" },
            { "Ice DMG", "IceAddedRatio" },
            { "Lightning DMG", "ThunderAddedRatio" },
            { "Wind DMG", "WindAddedRatio" },
            { "Quantum DMG", "QuantumAddedRatio" },
            { "Imaginary DMG", "ImaginaryAddedRatio" }
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
            { "CriticalChance", true },
            { "CriticalDamage", true },
            { "StatusProbability", true },
            { "StatusResistance", true },
            { "BreakDamageAddedRatio", true },
            { "SPRatioBase", true },
            { "HealRatioBase", true },
            { "PhysicalAddedRatio", true },
            { "FireAddedRatio", true },
            { "IceAddedRatio", true },
            { "ThunderAddedRatio", true },
            { "WindAddedRatio", true },
            { "QuantumAddedRatio", true },
            { "ImaginaryAddedRatio", true },
            { "CriticalChanceBase", true },
            { "CriticalDamageBase", true },
            { "BreakDamageAddedRatioBase", true }
        };

        static HSRStatPropertyUtils()
        {
            foreach (var kvp in DisplayNameToPropertyType)
            {
                PropertyTypeToDisplayName[kvp.Value] = kvp.Key;
            }
        }

        public static bool IsPercentageType(string propertyType)
        {
            return IsPercentProperty.TryGetValue(propertyType, out bool isPercent) && isPercent;
        }

        public static string GetDisplayName(string propertyType)
        {
            return PropertyTypeToDisplayName.TryGetValue(propertyType, out string displayName) 
                ? displayName 
                : propertyType;
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
                value *= 100;
                return $"{value:F1}%";
            }
            else if (propertyType == "SpeedDelta")
            {
                return $"{value:F1}";
            }
            else
            {
                return $"{(int)value}";
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
                case "HPDelta": return StatPropertyType.HPDelta;
                case "HPAddedRatio": return StatPropertyType.HPAddedRatio;
                case "AttackDelta": return StatPropertyType.AttackDelta;
                case "AttackAddedRatio": return StatPropertyType.AttackAddedRatio;
                case "DefenceDelta": return StatPropertyType.DefenceDelta;
                case "DefenceAddedRatio": return StatPropertyType.DefenceAddedRatio;
                case "SpeedDelta": return StatPropertyType.SpeedDelta;
                case "CriticalChance": return StatPropertyType.CriticalChance;
                case "CriticalDamage": return StatPropertyType.CriticalDamage;
                case "StatusProbability": return StatPropertyType.StatusProbability;
                case "StatusResistance": return StatPropertyType.StatusResistance;
                case "BreakDamageAddedRatio": return StatPropertyType.BreakDamageAddedRatio;
                case "BaseHP": return StatPropertyType.BaseHP;
                case "BaseAttack": return StatPropertyType.BaseAttack;
                case "BaseDefence": return StatPropertyType.BaseDefence;
                case "BaseSpeed": return StatPropertyType.BaseSpeed;
                case "PhysicalAddedRatio": return StatPropertyType.PhysicalAddedRatio;
                case "FireAddedRatio": return StatPropertyType.FireAddedRatio;
                case "IceAddedRatio": return StatPropertyType.IceAddedRatio;
                case "ThunderAddedRatio": return StatPropertyType.LightningAddedRatio;
                case "WindAddedRatio": return StatPropertyType.WindAddedRatio;
                case "QuantumAddedRatio": return StatPropertyType.QuantumAddedRatio;
                case "ImaginaryAddedRatio": return StatPropertyType.ImaginaryAddedRatio;
                case "HealRatioBase": return StatPropertyType.HealRatioBase;
                case "SPRatioBase": return StatPropertyType.SPRatioBase;
                case "CriticalChanceBase": return StatPropertyType.CriticalChanceBase;
                case "CriticalDamageBase": return StatPropertyType.CriticalDamageBase;
                case "BreakDamageAddedRatioBase": return StatPropertyType.BreakDamageAddedRatioBase;
                default: return StatPropertyType.None;
            }
        }

        public static double GetPropertyScalingFactor(string propertyType)
        {
            switch (propertyType)
            {
                case "HPAddedRatio": return 100.0;
                case "AttackAddedRatio": return 100.0;
                case "DefenceAddedRatio": return 100.0;
                case "CriticalChance": return 100.0;
                case "CriticalDamage": return 100.0;
                case "StatusProbability": return 100.0;
                case "StatusResistance": return 100.0;
                default: return IsPercentageType(propertyType) ? 100.0 : 1.0;
            }
        }
    }
}