using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EnkaDotNet.Components.Genshin
{
    public class Constellation
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public string IconUrl { get; internal set; } = string.Empty;
        public int Position { get; internal set; }
    }
}