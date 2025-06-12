using System.Collections.Generic;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class Weapon : EquipmentBase
    {
        internal EnkaClientOptions Options { get; set; }
        public WeaponType Type { get; internal set; }
        public int Ascension { get; internal set; }
        public int Refinement { get; internal set; }
        public double BaseAttack { get; internal set; }
        public StatProperty SecondaryStat { get; internal set; }

        public KeyValuePair<string, string> FormattedBaseAttack =>
            FormatStat(StatType.BaseAttack, BaseAttack);

        public KeyValuePair<string, string> FormattedSecondaryStat =>
            SecondaryStat != null ? FormatStat(SecondaryStat.Type, SecondaryStat.Value)
            : new KeyValuePair<string, string>(Options?.Raw ?? false ? "None" : "None", "0");

        private KeyValuePair<string, string> FormatStat(StatType type, double value)
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? type.ToString() : GenshinStatUtils.GetDisplayName(type);
            string formattedValue = GenshinStatUtils.FormatValue(type, value, raw);
            return new KeyValuePair<string, string>(key, formattedValue);
        }
    }
}