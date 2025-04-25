using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Components.Genshin
{
    public class Artifact : EquipmentBase
    {
        internal EnkaClientOptions Options { get; set; }
        public string SetName { get; internal set; } = string.Empty;
        public ArtifactSlot Slot { get; internal set; }
        public StatProperty MainStat { get; internal set; }
        public List<StatProperty> SubStats { get; internal set; } = new List<StatProperty>();

        public KeyValuePair<string, string> FormattedMainStat =>
             MainStat != null ? FormatStat(MainStat.Type, MainStat.Value)
             : new KeyValuePair<string, string>(Options?.Raw ?? false ? "None" : "None", "0");

        public List<KeyValuePair<string, string>> FormattedSubStats =>
            SubStats?.Select(s => FormatStat(s.Type, s.Value)).ToList() ?? new List<KeyValuePair<string, string>>();

        private KeyValuePair<string, string> FormatStat(StatType type, double value)
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? type.ToString() : GenshinStatUtils.GetDisplayName(type);
            string formattedValue = GenshinStatUtils.FormatValue(type, value, raw);
            return new KeyValuePair<string, string>(key, formattedValue);
        }
    }
}