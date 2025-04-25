using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EnkaDotNet.Enums.Genshin;
using EnkaDotNet.Utils.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class Character
    {
        internal EnkaClientOptions Options { get; set; }
        public int Id { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public int Level { get; internal set; }
        public int Ascension { get; internal set; }
        public int Friendship { get; internal set; }
        public ElementType Element { get; internal set; }
        public Dictionary<StatType, double> Stats { get; internal set; } = new Dictionary<StatType, double>();
        public List<int> UnlockedConstellationIds { get; internal set; } = new List<int>();
        public int ConstellationLevel { get; internal set; }
        public List<Talent> Talents { get; internal set; } = new List<Talent>();
        public List<Constellation> Constellations { get; internal set; } = new List<Constellation>();
        public Weapon Weapon { get; internal set; }
        public List<Artifact> Artifacts { get; internal set; } = new List<Artifact>();
        public int CostumeId { get; internal set; }
        public string IconUrl { get; internal set; } = string.Empty;


        public Dictionary<string, string> GetAllStats()
        {
            bool raw = this.Options?.Raw ?? false;
            var formatted = new Dictionary<string, string>();
            foreach (var kvp in Stats)
            {
                string key = raw ? kvp.Key.ToString() : GenshinStatUtils.GetDisplayName(kvp.Key);
                string value = GenshinStatUtils.FormatValueStats(kvp.Key, kvp.Value, raw);
                formatted.Add(key, value);
            }
            return formatted;
        }
    }
}