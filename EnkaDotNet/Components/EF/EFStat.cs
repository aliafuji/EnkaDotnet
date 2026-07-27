using EnkaDotNet.Enums.EF;
using EnkaDotNet.Utils.EF;

namespace EnkaDotNet.Components.EF
{
    public class EFStat
    {
        private string _name;
        private string _formattedValue;

        public EFAttrType Type { get; set; } = EFAttrType.Unknown;
        public int AttrId { get; set; }

        public string Name
        {
            get => !string.IsNullOrEmpty(_name) ? _name : EFStatsHelpers.GetAttrName(AttrId);
            set => _name = value;
        }

        public double Value { get; set; }

        public string FormattedValue
        {
            get => !string.IsNullOrEmpty(_formattedValue)
                ? _formattedValue
                : EFStatsHelpers.FormatAttrValue(AttrId, Value);
            set => _formattedValue = value;
        }

        public int EnhanceLevel { get; set; }
    }
}
