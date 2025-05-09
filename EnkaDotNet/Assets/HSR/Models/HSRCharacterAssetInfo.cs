using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRCharacterAssetInfo
    {
        [JsonPropertyName("AvatarName")]
        public AvatarNameInfo AvatarName { get; set; }

        [JsonPropertyName("AvatarFullName")]
        public AvatarNameInfo AvatarFullName { get; set; }

        [JsonPropertyName("AvatarSideIconPath")]
        public string AvatarSideIconPath { get; set; }

        [JsonPropertyName("ActionAvatarHeadIconPath")]
        public string ActionAvatarHeadIconPath { get; set; }

        [JsonPropertyName("AvatarCutinFrontImgPath")]
        public string AvatarCutinFrontImgPath { get; set; }

        [JsonPropertyName("AvatarBaseType")]
        public string AvatarBaseType { get; set; }

        [JsonPropertyName("Element")]
        public string Element { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("SkillList")]
        public List<int> SkillList { get; set; }

        [JsonPropertyName("RankIDList")]
        public List<int> RankIDList { get; set; }
    }

    public class AvatarNameInfo
    {
        [JsonPropertyName("Hash")]
        public string Hash { get; set; }
    }
}