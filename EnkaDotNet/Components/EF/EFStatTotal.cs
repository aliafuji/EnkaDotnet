using System;
using System.Globalization;

namespace EnkaDotNet.Components.EF
{
    public class EFStatTotal
    {
        public string Key { get; set; } = string.Empty;
        public string LocalizedName { get; set; } = string.Empty;
        public double Value { get; set; }
        public bool IsPercentage { get; set; }

        public double DisplayValue => IsPercentage
            ? Math.Round(Value, 1, MidpointRounding.AwayFromZero)
            : Math.Floor(Value + 1e-9);

        public double RawValue => IsPercentage ? Value / 100.0 : Value;

        public string FormattedValue => IsPercentage
            ? DisplayValue.ToString("0.0", CultureInfo.InvariantCulture) + "%"
            : DisplayValue.ToString(CultureInfo.InvariantCulture);
    }
}
