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
    public class HSRCharacterAssetInfo
    {
        [JsonProperty("AvatarName")]
        public AvatarNameInfo AvatarName { get; set; }

        [JsonProperty("AvatarFullName")]
        public AvatarNameInfo AvatarFullName { get; set; }

        [JsonProperty("AvatarSideIconPath")]
        public string AvatarSideIconPath { get; set; }

        [JsonProperty("ActionAvatarHeadIconPath")]
        public string ActionAvatarHeadIconPath { get; set; }

        [JsonProperty("AvatarCutinFrontImgPath")]
        public string AvatarCutinFrontImgPath { get; set; }

        [JsonProperty("AvatarBaseType")]
        public string AvatarBaseType { get; set; }

        [JsonProperty("Element")]
        public string Element { get; set; }

        [JsonProperty("Rarity")]
        public int Rarity { get; set; }

        [JsonProperty("SkillList")]
        public List<int> SkillList { get; set; }

        [JsonProperty("RankIDList")]
        public List<int> RankIDList { get; set; }
    }

    public class AvatarNameInfo
    {
        [JsonProperty("Hash")]
        public string Hash { get; set; }
    }
}
