using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRLightConeAssetInfo
    {
        [JsonProperty("EquipmentName")]
        public EquipmentNameInfo EquipmentName { get; set; }

        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; }

        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("AvatarBaseType")]
        public string AvatarBaseType { get; set; }
    }

    public class EquipmentNameInfo
    {
        [JsonProperty("Hash")]
        public string Hash { get; set; }
    }
}
