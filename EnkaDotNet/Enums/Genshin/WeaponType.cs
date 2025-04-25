using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Enums.Genshin
{
    public enum WeaponType
    {
        Unknown = 0,
        Sword = 1,      // WEAPON_SWORD_ONE_HAND
        Claymore = 2,   // WEAPON_CLAYMORE
        Polearm = 3,    // WEAPON_POLE
        Bow = 4,        // WEAPON_BOW
        Catalyst = 5    // WEAPON_CATALYST
    }
}
