using EnkaDotNet.Enums.ZZZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
    }
}