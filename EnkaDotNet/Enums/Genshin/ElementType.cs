using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Enums.Genshin
{
    public enum ElementType
    {
        Unknown = 0,
        Anemo,    // Wind
        Geo,      // Rock
        Electro,  // Elec
        Dendro,   // Grass
        Hydro,    // Water
        Pyro,     // Fire
        Cryo,     // Ice
        Physical // Often treated distinctly, though not a core element
    }
}
