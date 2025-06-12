using System;
using System.Collections.Generic;
using EnkaDotNet.Enums.HSR;
using System.Globalization;
using EnkaDotNet.Utils.HSR;

namespace EnkaDotNet.Components.HSR
{
    public class HSRLightCone
    {
        internal EnkaClientOptions Options { get; set; }

        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Promotion { get; internal set; }
        public int Rank { get; internal set; }

        public PathType Path { get; internal set; }
        public int Rarity { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;

        public List<HSRStatProperty> Properties { get; internal set; } = new List<HSRStatProperty>();
        public double BaseHP { get; internal set; }
        public double BaseAttack { get; internal set; }
        public double BaseDefense { get; internal set; }

        public override string ToString()
        {
            return $"{Name} (Lv.{Level}/{Promotion}, Rank {Rank})";
        }

        public KeyValuePair<string, string> FormattedBaseHP => GetFormattedStat("BaseHP", BaseHP);
        public KeyValuePair<string, string> FormattedBaseAttack => GetFormattedStat("BaseAttack", BaseAttack);
        public KeyValuePair<string, string> FormattedBaseDefense => GetFormattedStat("BaseDefence", BaseDefense);


        private KeyValuePair<string, string> GetFormattedStat(string propertyType, double value)
        {
            bool raw = Options?.Raw ?? false;
            string key = raw ? propertyType : HSRStatPropertyUtils.GetDisplayName(propertyType);
            string formattedValue = raw ? value.ToString(CultureInfo.InvariantCulture) : Math.Floor(value).ToString();
            return new KeyValuePair<string, string>(key, formattedValue);
        }
    }
}