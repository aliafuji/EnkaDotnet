using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;
using System.Collections.Generic;

namespace EnkaDotNet.Components.Genshin
{
    public class Artifact : EquipmentBase
    {
        internal EnkaClientOptions Options { get; set; }
        public string SetName { get; internal set; } = string.Empty;
        public ArtifactSlot Slot { get; internal set; }
        public StatProperty MainStat { get; internal set; }
        public IReadOnlyList<StatProperty> SubStats { get; internal set; } = new List<StatProperty>();

        public KeyValuePair<string, string> FormattedMainStat =>
             MainStat != null ? FormatStat(MainStat.Type, MainStat.Value)
             : new KeyValuePair<string, string>(Options?.Raw ?? false ? "None" : "None", "0");

        public List<KeyValuePair<string, string>> FormattedSubStats
        {
            get
            {
                var formattedStats = new List<KeyValuePair<string, string>>();
                if (SubStats != null)
                {
                    foreach (var s in SubStats)
                    {
                        formattedStats.Add(FormatStat(s.Type, s.Value));
                    }
                }
                return formattedStats;
            }
        }


        private KeyValuePair<string, string> FormatStat(StatType type, double value)
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? type.ToString() : GenshinStatUtils.GetDisplayName(type);
            string formattedValue = GenshinStatUtils.FormatValue(type, value, raw);
            return new KeyValuePair<string, string>(key, formattedValue);
        }
    }
}
