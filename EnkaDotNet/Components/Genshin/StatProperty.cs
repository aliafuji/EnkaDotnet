using EnkaDotNet.Enums.Genshin;
using System.Globalization;

namespace EnkaDotNet.Components.Genshin;

public class StatProperty
{
    public StatType Type { get; internal set; }

    public double Value { get; internal set; }

    public override string ToString()
    {
        bool isPercent = Type.ToString().Contains("Percentage") ||
                         Type.ToString().Contains("Bonus") ||
                         Type == StatType.CriticalRate ||
                         Type == StatType.CriticalDamage ||
                         Type == StatType.EnergyRecharge;

        string valueString;
        if (isPercent)
        {
            valueString = (Value / 100.0).ToString("P1", CultureInfo.InvariantCulture);
        }
        else if (Value == (int)Value)
        {
            valueString = Value.ToString("N0", CultureInfo.InvariantCulture);
        }
        else
        {
            valueString = Value.ToString("N1", CultureInfo.InvariantCulture);
        }

        return $"{Type}: {valueString}";
    }
}
