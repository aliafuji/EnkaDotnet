using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZStat
    {
        public StatType Type { get; set; }
        public double Value { get; set; }
        public double FormattedValue { get; set; }
        public int Level { get; set; }
        public bool IsPercentage { get; internal set; }

        public string DisplayValue => IsPercentage ? $"{Value:F1}%" : $"{(int)Value}";

        public override string ToString()
        {
            return $"{Type}: {DisplayValue}";
        }
    }
}