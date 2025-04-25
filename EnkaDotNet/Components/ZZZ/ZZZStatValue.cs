using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZStatValue
    {
        public double Raw { get; set; }
        public string Formatted { get; set; }
        public bool IsPercentage { get; set; }
        public bool IsEnergyRegen { get; set; }

        public ZZZStatValue(double raw, bool isPercentage = false, bool isEnergyRegen = false)
        {
            Raw = raw;
            IsPercentage = isPercentage;
            IsEnergyRegen = isEnergyRegen;

            if (isEnergyRegen)
            {
                Formatted = $"{raw:F2}";
            }
            else if (isPercentage)
            {
                Formatted = $"{raw:F1}%";
            }
            else
            {
                Formatted = $"{Math.Floor(raw)}";
            }
        }

        public override string ToString() => Formatted;
    }
}