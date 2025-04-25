using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Components.HSR.EnkaDotNet.Enums.HSR;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Utils.HSR;

namespace EnkaDotNet.Components.HSR
{
    public class HSRStatProperty
    {
        public string Type { get; set; } = string.Empty;
        public StatPropertyType PropertyType { get; set; }
        public double Value { get; set; }
        public double BaseValue { get; set; }
        public bool IsPercentage { get; set; }

        public string DisplayValue
        {
            get
            {
                if (IsPercentage)
                {
                    return $"{Value * 100:F1}%";
                }
                else if (Type == "SpeedDelta")
                {
                    return $"{Value:F1}";
                }
                else
                {
                    return $"{(int)Value}";
                }
            }
        }

        public double CalculationValue
        {
            get
            {
                return IsPercentage ? Value : Value;
            }
        }

        public override string ToString()
        {
            return $"{HSRStatPropertyUtils.GetDisplayName(Type)}: {DisplayValue}";
        }
    }

    public class HSRStatValue
    {
        public double Raw { get; set; }
        public double FormattedValue { get; set; }
        public string Formatted { get; set; }
        public bool IsPercentage { get; set; }

        public HSRStatValue(double raw, bool isPercentage = false, int decimalPlaces = 0)
        {
            Raw = raw;
            FormattedValue = raw;
            IsPercentage = isPercentage;

            if (isPercentage)
            {
                Formatted = $"{raw:F1}%";
            }
            else if (decimalPlaces > 0)
            {
                string format = $"F{decimalPlaces}";
                Formatted = $"{raw.ToString(format)}";
            }
            else
            {
                Formatted = $"{(int)raw}";
            }
        }

        public override string ToString() => Formatted;
    }

    namespace EnkaDotNet.Enums.HSR
    {
        public enum TraceType
        {
            Unknown = 0,
            BasicAttack = 1,
            Skill = 2,
            Ultimate = 3,
            Talent = 4,
            Technique = 5,
            StatBoost = 6,
            MajorStatBoost = 7
        }
    }

    public class HSRSkillTree
    {
        public int PointId { get; set; }
        public int Level { get; set; }
        public int BaseLevel { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TraceType TraceType { get; set; } = TraceType.Unknown;
        public bool IsBoosted { get; set; } = false;
        public int MaxLevel { get; set; } = 1;
        public string Anchor { get; set; } = string.Empty;
        public List<int> SkillIds { get; set; } = new List<int>();

        public override string ToString()
        {
            string levelInfo = IsBoosted ? $"{BaseLevel}+{Level - BaseLevel}={Level}" : $"{Level}";
            return $"{Name} ({TraceType}) - Lv.{levelInfo}/{MaxLevel}";
        }
    }
}
