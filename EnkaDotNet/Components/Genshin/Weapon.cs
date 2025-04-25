using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class Weapon : EquipmentBase
    {
        public WeaponType Type { get; internal set; }
        public int Ascension { get; internal set; }
        public int Refinement { get; internal set; }
        public double BaseAttack { get; internal set; }
        public StatProperty SecondaryStat { get; internal set; }
    }
}