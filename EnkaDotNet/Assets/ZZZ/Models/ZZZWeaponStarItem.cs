using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZWeaponStarData
    {
        [JsonPropertyName("Items")]
        public List<ZZZWeaponStarItem> Items { get; set; }
    }

    public class ZZZWeaponStarItem
    {
        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("BreakLevel")]
        public int BreakLevel { get; set; }

        [JsonPropertyName("StarRate")]
        public double StarRate { get; set; }

        [JsonPropertyName("RandRate")]
        public double RandRate { get; set; }

        [JsonPropertyName("UnlockLevel")]
        public int UnlockLevel { get; set; }

        [JsonPropertyName("Exp")]
        public int Exp { get; set; }
    }
}