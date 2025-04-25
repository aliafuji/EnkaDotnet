using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.Genshin.Models
{
    public class CharacterAssetInfo
    {
        [JsonProperty("NameTextMapHash")]
        public string NameTextMapHash { get; set; }

        [JsonProperty("Element")]
        public string Element { get; set; }

        [JsonProperty("WeaponType")]
        public string WeaponType { get; set; }

        [JsonProperty("SideIconName")]
        public string SideIconName { get; set; }

        [JsonProperty("NamecardIcon")]
        public string NamecardIcon { get; set; }

        [JsonProperty("Consts")]
        public List<string> Constellations { get; set; }
    }
}