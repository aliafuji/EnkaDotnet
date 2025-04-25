using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.ZZZ;

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
        public List<ZZZStat> SubStats { get; internal set; } = new List<ZZZStat>();
    }
}