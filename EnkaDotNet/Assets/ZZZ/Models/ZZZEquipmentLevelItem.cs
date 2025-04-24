using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZEquipmentLevelData
    {
        [JsonPropertyName("Items")]
        public List<ZZZEquipmentLevelItem>? Items { get; set; }
    }

    public class ZZZEquipmentLevelItem
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("EnhanceRate")]
        public double EnhanceRate { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }

        [JsonPropertyName("ExpRecycleRate")]
        public int ExpRecycleRate { get; set; }
    }
}