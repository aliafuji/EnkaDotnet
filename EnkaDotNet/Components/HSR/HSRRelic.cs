using System.Collections.Generic;
using System.Linq;
using EnkaDotNet.Enums.HSR;
using EnkaDotNet.Utils.HSR;
using System.Globalization;

namespace EnkaDotNet.Components.HSR
{
    public class HSRRelic
    {
        internal EnkaClientOptions Options { get; set; }

        public int Id { get; internal set; }
        public int Level { get; internal set; }
        public int Type { get; internal set; }
        public RelicType RelicType { get; internal set; }

        public int SetId { get; internal set; }
        public string SetName { get; internal set; } = string.Empty;
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;

        public HSRStatProperty MainStat { get; internal set; } = new HSRStatProperty();
        public List<HSRStatProperty> SubStats { get; internal set; } = new List<HSRStatProperty>();

        public override string ToString()
        {
            return $"{SetName} {RelicType} (Lv.{Level})";
        }

        public KeyValuePair<string, string> FormattedMainStat => GetAllStats(MainStat);

        public List<KeyValuePair<string, string>> FormattedSubStats => SubStats.Select(GetAllStats).ToList();

        private KeyValuePair<string, string> GetAllStats(HSRStatProperty stat)
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? stat.Type : HSRStatPropertyUtils.GetDisplayName(stat.Type);
            string value = raw ? stat.Value.ToString(CultureInfo.InvariantCulture) : stat.DisplayValue;
            return new KeyValuePair<string, string>(key, value);
        }
    }
}