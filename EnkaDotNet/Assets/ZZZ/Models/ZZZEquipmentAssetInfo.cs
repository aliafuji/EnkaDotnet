using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZEquipmentData
    {
        [JsonPropertyName("Items")]
        public Dictionary<string, ZZZEquipmentItemInfo>? Items { get; set; }

        [JsonPropertyName("Suits")]
        public Dictionary<string, ZZZEquipmentSuitInfo>? Suits { get; set; }
    }

    public class ZZZEquipmentItemInfo
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("SuitId")]
        public int SuitId { get; set; }
    }

    public class ZZZEquipmentSuitInfo
    {
        [JsonPropertyName("Icon")]
        public string? Icon { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("SetBonusProps")]
        public Dictionary<string, int>? SetBonusProps { get; set; }
    }
}