using EnkaDotNet.Enums.ZZZ;
using System;
using System.Collections.Generic;
using EnkaDotNet.Utils.ZZZ;
using System.Globalization;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZWEngine
    {
        public string Uid { get; internal set; } = string.Empty;
        public int Id { get; internal set; }
        public int Level { get; internal set; }
        public int BreakLevel { get; internal set; }
        public int UpgradeLevel { get; internal set; }
        public bool IsAvailable { get; internal set; }
        public bool IsLocked { get; internal set; }

        public string Name { get; internal set; } = string.Empty;
        public Rarity Rarity { get; internal set; }
        public ProfessionType ProfessionType { get; internal set; }
        public string ImageUrl { get; internal set; } = string.Empty;

        public ZZZStat MainStat { get; internal set; } = new ZZZStat();
        public ZZZStat SecondaryStat { get; internal set; } = new ZZZStat();

        internal EnkaClientOptions Options { get; set; }

        public KeyValuePair<string, string> FormattedMainStat
        {
            get
            {
                bool raw = Options?.Raw ?? false;
                string key = raw ? MainStat.Type.ToString() : ZZZStatsHelpers.GetStatCategoryDisplay(MainStat.Type);
                string value = Math.Floor(MainStat.Value).ToString(CultureInfo.InvariantCulture);
                return new KeyValuePair<string, string>(key, value);
            }
        }

        public KeyValuePair<string, string> FormattedSecondaryStat
        {
            get
            {
                bool raw = Options?.Raw ?? false;
                string key = raw ? SecondaryStat.Type.ToString() : ZZZStatsHelpers.GetStatCategoryDisplay(SecondaryStat.Type);
                string value;

                if (raw)
                {
                    if (SecondaryStat.Type == StatType.EnergyRegenPercent) value = Math.Floor(SecondaryStat.Value).ToString(CultureInfo.InvariantCulture);
                    else if (SecondaryStat.IsPercentage) value = SecondaryStat.Value.ToString("F1", CultureInfo.InvariantCulture);
                    else value = Math.Floor(SecondaryStat.Value).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    if (SecondaryStat.Type == StatType.EnergyRegenPercent) value = (SecondaryStat.Value / 100.0).ToString("F1", CultureInfo.InvariantCulture) + "%";
                    else if (SecondaryStat.IsPercentage) value = SecondaryStat.Value.ToString("F1", CultureInfo.InvariantCulture) + "%";
                    else value = Math.Floor(SecondaryStat.Value).ToString(CultureInfo.InvariantCulture);
                }
                return new KeyValuePair<string, string>(key, value);
            }
        }
    }
}
