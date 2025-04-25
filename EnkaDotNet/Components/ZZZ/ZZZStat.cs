using EnkaDotNet.Enums.ZZZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Utils.ZZZ;
using System.Globalization;

namespace EnkaDotNet.Components.ZZZ
{
    public class ZZZStat
    {
        public StatType Type { get; set; } = StatType.None;
        public double Value { get; set; }
        public int Level { get; set; }
        public bool IsPercentage { get; internal set; }
    }
}