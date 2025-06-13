using System;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZStatValue
    {
        public double Raw { get; set; }
        public string Formatted { get; private set; }
        public bool IsPercentage { get; private set; }
        public bool IsEnergyRegen { get; private set; }

        public ZZZStatValue(double raw, bool isPercentage = false, bool isEnergyRegen = false)
        {
            Raw = raw;
            IsPercentage = isPercentage;
            IsEnergyRegen = isEnergyRegen;

            if (isEnergyRegen) Formatted = $"{raw:F2}";
            else if (isPercentage) Formatted = $"{raw:F1}%";
            else Formatted = $"{Math.Floor(raw)}";
        }

        public override string ToString() => Formatted;
    }
}
