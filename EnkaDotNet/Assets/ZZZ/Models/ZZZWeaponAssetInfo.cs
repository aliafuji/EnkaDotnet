using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZWeaponAssetInfo
    {
        [JsonPropertyName("ItemName")]
        public string ItemName { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("ProfessionType")]
        public string ProfessionType { get; set; }

        [JsonPropertyName("ImagePath")]
        public string ImagePath { get; set; }

        [JsonPropertyName("MainStat")]
        public ZZZStatProperty MainStat { get; set; }

        [JsonPropertyName("SecondaryStat")]
        public ZZZStatProperty SecondaryStat { get; set; }
    }

    public class ZZZStatProperty
    {
        [JsonPropertyName("PropertyId")]
        public int PropertyId { get; set; }

        [JsonPropertyName("PropertyValue")]
        public int PropertyValue { get; set; }
    }
}