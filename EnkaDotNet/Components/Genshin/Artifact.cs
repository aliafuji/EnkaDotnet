using EnkaDotNet.Enums.Genshin;
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
        public string SetName { get; internal set; } = string.Empty;
        public ArtifactSlot Slot { get; internal set; }
        public StatProperty MainStat { get; internal set; }
        public List<StatProperty> SubStats { get; internal set; } = new List<StatProperty>();
    }
}