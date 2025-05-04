using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.ZZZ;
using EnkaDotNet.Utils.ZZZ;
using System.Globalization;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZDriveDisc
    {
        public string Uid { get; internal set; } = string.Empty;
        public int Id { get; internal set; }
        public int Level { get; internal set; }
        public int BreakLevel { get; internal set; }
        public bool IsLocked { get; internal set; }
        public bool IsAvailable { get; internal set; }
        public bool IsTrash { get; internal set; }
        public DriveDiscSlot Slot { get; internal set; }

        public Rarity Rarity { get; internal set; }
        public int SuitId { get; internal set; }
        public string SuitName { get; internal set; } = string.Empty;
        public string IconUrl { get; internal set; } = string.Empty;

        public ZZZStat MainStat { get; internal set; } = new ZZZStat();
        public List<ZZZStat> SubStatsRaw { get; internal set; } = new List<ZZZStat>();

        internal EnkaClientOptions Options { get; set; }

        public KeyValuePair<string, string> FormattedMainStat
        {
            get
            {
                bool raw = Options?.Raw ?? false;
                string key = raw ? MainStat.Type.ToString() : ZZZStatsHelpers.GetStatCategoryDisplay(MainStat.Type);
                string value;
                if (MainStat.IsPercentage && !raw) value = (MainStat.Value).ToString("F1", CultureInfo.InvariantCulture) + "%";
                else if (MainStat.IsEnergyRegen && !raw) value = (MainStat.Value / 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
                else if (MainStat.IsEnergyRegen && raw) value = (MainStat.Value / 100).ToString("F1", CultureInfo.InvariantCulture);
                else value = Math.Floor(MainStat.Value).ToString();
                return new KeyValuePair<string, string>(key, value);
            }
        }

        public List<KeyValuePair<string, string>> SubStats
        {
            get
            {
                bool raw = Options?.Raw ?? false;
                var formattedList = new List<KeyValuePair<string, string>>();
                foreach (var stat in SubStatsRaw)
                {
                    string key = raw ? stat.Type.ToString() : ZZZStatsHelpers.GetStatCategoryDisplay(stat.Type);
                    double totalValue = stat.Value * stat.Level;
                    string value;
                    if (stat.IsPercentage && !raw) value = (totalValue).ToString("F1", CultureInfo.InvariantCulture) + "%";
                    else if (stat.IsEnergyRegen && !raw) value = (totalValue / 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
                    else if (stat.IsEnergyRegen && raw) value = (totalValue / 100).ToString("F1", CultureInfo.InvariantCulture);
                    else value = Math.Floor(totalValue).ToString();
                    formattedList.Add(new KeyValuePair<string, string>(key, $"{value} +{stat.Level}"));
                }
                return formattedList;
            }
        }
    }
}