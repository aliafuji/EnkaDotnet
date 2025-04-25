using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.HSR;

namespace EnkaDotNet.Components.HSR
{
    public class HSRRelic
    {
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
    }
}
