using System.Text.Json.Serialization;

namespace EnkaDotNet.Assets.ZZZ.Models
{
    public class ZZZAvatarAssetInfo
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("ProfessionType")]
        public string? ProfessionType { get; set; }

        [JsonPropertyName("ElementTypes")]
        public List<string>? ElementTypes { get; set; }

        [JsonPropertyName("Image")]
        public string? Image { get; set; }

        [JsonPropertyName("CircleIcon")]
        public string? CircleIcon { get; set; }

        [JsonPropertyName("WeaponId")]
        public int WeaponId { get; set; }

        [JsonPropertyName("Colors")]
        public ZZZAvatarColors? Colors { get; set; }

        [JsonPropertyName("HighlightProps")]
        public List<int>? HighlightProps { get; set; }

        [JsonPropertyName("BaseProps")]
        public Dictionary<string, int>? BaseProps { get; set; }

        [JsonPropertyName("GrowthProps")]
        public Dictionary<string, int>? GrowthProps { get; set; }

        [JsonPropertyName("PromotionProps")]
        public List<Dictionary<string, int>>? PromotionProps { get; set; }

        [JsonPropertyName("CoreEnhancementProps")]
        public List<Dictionary<string, int>>? CoreEnhancementProps { get; set; }
    }

    public class ZZZAvatarColors
    {
        [JsonPropertyName("Accent")]
        public string? Accent { get; set; }

        [JsonPropertyName("Mindscape")]
        public string? Mindscape { get; set; }
    }
}