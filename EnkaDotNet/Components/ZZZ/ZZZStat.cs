using EnkaDotNet.Enums.ZZZ;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZStat
    {
        public StatType Type { get; set; } = StatType.None;
        public double Value { get; set; }
        public int Level { get; set; }
        public bool IsPercentage { get; internal set; }
        public bool IsEnergyRegen { get; internal set; }
    }
}
