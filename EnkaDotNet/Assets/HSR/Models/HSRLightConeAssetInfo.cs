using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.HSR.Models
{
    public class HSRLightConeAssetInfo
    {
        [JsonPropertyName("EquipmentName")]
        public EquipmentNameInfo EquipmentName { get; set; }

        [JsonPropertyName("ImagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("AvatarBaseType")]
        public string AvatarBaseType { get; set; }
    }

    public class EquipmentNameInfo
    {
        [JsonPropertyName("Hash")]
        public string Hash { get; set; }
    }
}