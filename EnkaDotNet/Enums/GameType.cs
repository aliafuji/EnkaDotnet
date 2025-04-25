using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Enums
{
    public enum GameType
    {
        [Description("Genshin Impact")]
        Genshin = 0,

        [Description("Zenless Zone Zero")]
        ZZZ = 1,

        [Description("Honkai Star Rail")]
        HSR = 2,
    }
}