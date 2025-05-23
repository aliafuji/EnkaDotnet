﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Enums.Genshin
{
    public enum ArtifactSlot
    {
        Unknown = 0,
        Flower = 1,     // EQUIP_BRACER
        Plume = 2,      // EQUIP_NECKLACE
        Sands = 3,      // EQUIP_SHOES
        Goblet = 4,     // EQUIP_RING
        Circlet = 5     // EQUIP_DRESS
    }
}
