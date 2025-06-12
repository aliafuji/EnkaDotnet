using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class StatProperty
    {
        internal EnkaClientOptions Options { get; set; }
        public StatType Type { get; internal set; }
        public double Value { get; internal set; }

        public override string ToString()
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? Type.ToString() : GenshinStatUtils.GetDisplayName(Type);
            string value = GenshinStatUtils.FormatValueStats(Type, Value, raw);
            return $"{key}: {value}";
        }

        public string FormattedValue => GenshinStatUtils.FormatValueStats(Type, Value, Options?.Raw ?? false);
        public string DisplayName => GenshinStatUtils.GetDisplayName(Type);
    }
}